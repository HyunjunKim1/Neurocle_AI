using Common;
using HDSInspector_AI.Class.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI.Class.Manager
{
    /// <summary>   DB 관리  Class       </summary>
    /// <remarks>   hjkim, 2026-08-21.   </remarks>
    /// 
    public class DatabaseManager : IDisposable
    {
        private readonly object _syncLock = new object();

        public bool IsInitialized { get; private set; }
        public string LastError { get; private set; }

        // DB가 없는 상태에서도 연결할 수 있는 Connection string
        private string CreateServerConnectionString()
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
            {
                Server = GLB.Setting.Database.Server,
                Port   = (uint)GLB.Setting.Database.Port,
                UserID = GLB.Setting.Database.User,
                Password = GLB.Setting.Database.Password,
                ConnectionTimeout = (uint)GLB.Setting.Database.ConnectionTimeout,
                CharacterSet = "utf8mb4",
            };

            return builder.ConnectionString;
        }

        // DB 연결
        private string CreateDatabaseConnectionString()
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
            {
                Server = GLB.Setting.Database.Server,
                Port = (uint)GLB.Setting.Database.Port,
                UserID = GLB.Setting.Database.User,
                Password = GLB.Setting.Database.Password,
                ConnectionTimeout = (uint)GLB.Setting.Database.ConnectionTimeout,
                CharacterSet = "utf8mb4",
            };

            return builder.ConnectionString;
        }

        public bool Initialize()
        {
            lock(_syncLock)
            {
                LastError = string .Empty;
                IsInitialized = false;

                try
                {
                    // DB 생성
                    if (!CreateDatabase()) return false;
                    if (!CreateTables()) return false;

                    SaveSchemaFile();
                    IsInitialized = true;

                    GLB.AddLog("DATABASE", $"MySQL Initialize complete : {GLB.Setting.Database.Server}:{GLB.Setting.Database.Port}", SeverityLevel.INFO);

                    return true;
                }
                catch(Exception ex)
                {
                    LastError = ex.ToString();
                    GLB.AddLog("DATABASE", $"MySQL Initialize Failed : {GLB.Setting.Database.Server}:{GLB.Setting.Database.Port}, {ex.Message}", SeverityLevel.ERROR);

                    return false;
                }
            }
        }
        public void Dispose()
        {
            IsInitialized = false;
        }


        private bool CreateDatabase()
        {
            try
            {
                using(MySqlConnection conn = new MySqlConnection(CreateServerConnectionString()))
                {
                    conn.Open();

                    string dbName = GLB.Setting.Database.DatabaseName;

                    //DBName은 Ini에서 string 형태로 들어옴.
                    string sql = $"CREATE DATABASE IF NOT EXISTS '{dbName}' " + "CHARACTER SET utf8mb4 " + "COLLATE utf8mb4_unicode_ci;";

                    using(MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = GLB.Setting.Database.CommandTimeout;
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch(Exception ex)
            {
                LastError = ex.Message;

                return false;
            }
        }

        private bool CreateTables()
        {
            try
            {
                using(MySqlConnection conn = new MySqlConnection(CreateServerConnectionString()))
                {
                    conn.Open();

                    ExecuteNonQuery(conn, CreateLotTableSql);
                    ExecuteNonQuery(conn, CreateStripTableSql);
                    ExecuteNonQuery(conn, CreateDefectTableSql);
                }

                return true;
            }
            catch (Exception ex)
            {
                LastError = $"Table 생성 실패 : {ex.Message}";

                return false;
            }
        }

        private void ExecuteNonQuery(MySqlConnection conn, string sql)
        {
            using(MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.CommandTimeout = GLB.Setting.Database.CommandTimeout;
                cmd.ExecuteNonQuery();
            }
        }

        private void SaveSchemaFile()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("-- Neurocle AI Database Schema");
            builder.AppendLine("-- Auto Generated");
            builder.AppendLine();

            builder.AppendLine(CreateLotTableSql);
            builder.AppendLine();

            builder.AppendLine(CreateStripTableSql);
            builder.AppendLine();

            builder.AppendLine(CreateDefectTableSql);
            File.WriteAllText(GLB.Setting.Database.SchemaFilePath, builder.ToString(), Encoding.UTF8);
        }

        public bool TestConnection(out string errorMessage)
        {
            errorMessage = null;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(CreateDatabaseConnectionString()))
                { 
                    conn.Open();

                    using (MySqlCommand cmd = new MySqlCommand("SELECT 1;", conn))
                    {
                        cmd.ExecuteScalar();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public bool SaveInspectionResult(InspectionInfo inspectionInfo, StripInferenceResult stripResult)
        {
            LastError = string.Empty;

            if (!IsInitialized) return false;
            if (inspectionInfo == null || !inspectionInfo.IsValid) return false;
            if (stripResult == null) return false;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(CreateDatabaseConnectionString()))
                {
                    conn.Open();

                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Lot
                            long lotId = GetOrCreateLot(conn, transaction, inspectionInfo);

                            // strip
                            long stripId = InsertOrUpdateStrip(conn, transaction, lotId, stripResult);

                            // 기존 Defect 제거, 동일 Strip 재처리를 고려해서 다시 Insert하기 전에 제거해버리자
                            DeleteStripDefects(conn, transaction, stripId);

                            // Defect 저장
                            foreach (DefectInferenceResult defect in stripResult.Results)
                            {
                                if (defect == null) continue;
                                InsertDefect(conn, transaction, stripId, defect);
                            }

                            transaction.Commit();

                            GLB.AddLog("DATABASE", $"Strip DB Save Success : [{stripResult.StripNumber}], Count = {stripResult.TotalCount}", SeverityLevel.INFO);

                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();

                            throw;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;

                GLB.AddLog("DATABASE", $"Strip DB Save Failed : [{stripResult.StripNumber:D6}], {ex.Message}", SeverityLevel.ERROR);

                return false;
            }
        }

        public bool BackupDatabase()
        {
            LastError = string.Empty;

            if (!GLB.Setting.Database.BackupEnable) return true;

            try
            {
                if(!File.Exists(GLB.Setting.Database.MySqlDumpPath))
                {
                    LastError = $"mysqldump.exe가 없습니다. : {GLB.Setting.Database.MySqlDumpPath}";

                    return false;
                }

                Directory.CreateDirectory(GLB.Setting.Database.BackupDirectory);

                string fileName = "AIJudgement_" + $"{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                string backupPath = Path.Combine(GLB.Setting.Database.BackupDirectory, fileName);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = GLB.Setting.Database.MySqlDumpPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                string arguments = $"-h\"{GLB.Setting.Database.Server}\" " +
                                   $"-P{GLB.Setting.Database.Port} " +
                                   $"-u\"{GLB.Setting.Database.User}\" ";

                if (!string.IsNullOrEmpty(GLB.Setting.Database.Password))
                    arguments += $"-p\"{GLB.Setting.Database.Password}\" ";

                arguments += $"--single-transaction " +
                             $"--routines --triggers " +
                             $"\"{GLB.Setting.Database.DatabaseName}\"";

                using(Process process = Process.Start(startInfo))
                {
                    using(FileStream stream = new FileStream(backupPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        process.StandardOutput.BaseStream.CopyTo(stream);
                    }

                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if(process.ExitCode != 0)
                    {
                        DeleteIfExists(backupPath);

                        LastError = error;

                        return false;
                    }
                }

                CleanOldBackups();

                GLB.AddLog("DATABASE", $"Database Backup Success : {backupPath}", SeverityLevel.INFO);

                return true;
            }

            catch (Exception ex)
            {
                LastError = ex.Message;

                return false;
            }
        }

        private void CleanOldBackups()
        {
            if (GLB.Setting.Database.BackupKeepDays <= 0) return;

            DateTime expireTime = DateTime.Now.AddDays(-GLB.Setting.Database.BackupKeepDays);
            string[] files = Directory.GetFiles(GLB.Setting.Database.BackupDirectory, "AIJudgement_*.sql");

            foreach(string file in files)
            {
                try
                {
                    FileInfo info = new FileInfo(file);
                    if (info.LastWriteTime < expireTime)
                        info.Delete();
                }
                catch { }
            }
        }

        private static void DeleteIfExists(string path)
        {
            if(!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                File.Delete(path); 
        }

        #region Querys

        private long GetOrCreateLot(MySqlConnection connection, MySqlTransaction transaction, InspectionInfo info)
        {
            string selectSql = "SELECT id FROM inspection_lot WHERE device_name = device AND product_name = product AND order_number = order ORDER BY id DESC LIMIT 1;";
            using (MySqlCommand command = new MySqlCommand(selectSql, connection, transaction))
            {
                command.Parameters.AddWithValue("@device", info.DeviceName);
                command.Parameters.AddWithValue("@product", info.ProductName);
                command.Parameters.AddWithValue("@order", info.OrderNumber);

                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    return Convert.ToInt64(result);
            }


            /*
             * 없으면 신규 생성
             */
            string insertSql = "INSERT INTO inspection_lot (device_name, product_name, order_number, start_time) VALUES(device, product, order, start_time); SELECT LAST_INSERT_ID();";
            using (MySqlCommand command = new MySqlCommand(insertSql, connection, transaction))
            {
                command.Parameters.AddWithValue("device", info.DeviceName);
                command.Parameters.AddWithValue("product",info.ProductName);
                command.Parameters.AddWithValue("@order",info.OrderNumber);
                command.Parameters.AddWithValue("@start_time",DateTime.Now);

                return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        private long InsertOrUpdateStrip(MySqlConnection connection, MySqlTransaction transaction, long lotId, StripInferenceResult result)
        {
            string sql = "INSERT INTO inspection_strip(lot_id, strip_number, total_count, ok_count, ng_count, unknown_count, processing_time_ms, inspected_at) " +
                         "VALUES(lot_id, strip_number, total_count, ok_count, ng_count, unknown_count, processing_time_ms, inspected_at) " +
                         "ON DUPLICATE KEY UPDATE total_count = VALUES(total_count), ok_count = VALUES(ok_count), ng_count = VALUES(ng_count), unknown_count = VALUES(unknown_count), processing_time_ms = VALUES(processing_time_ms), inspected_at = VALUES(inspected_at), id = LAST_INSERT_ID(id); SELECT LAST_INSERT_ID();";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("lot_id", lotId);
                command.Parameters.AddWithValue("strip_number",result.StripNumber);
                command.Parameters.AddWithValue("total_count",result.TotalCount);
                command.Parameters.AddWithValue("ok_count",result.OKCount);
                command.Parameters.AddWithValue("ng_count",result.NGCount);
                command.Parameters.AddWithValue("unknown_count",result.UnknownCount);
                command.Parameters.AddWithValue("processing_time_ms",result.ProcessingTimeMs);
                command.Parameters.AddWithValue("inspected_at",DateTime.Now);

                return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        private void DeleteStripDefects(MySqlConnection conn, MySqlTransaction trans, long stripId)
        {
            const string sql = "DELETE FROM inspection_defect WHERE strip_id = strip_id;";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("strip_id", stripId);
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertDefect(MySqlConnection connection, MySqlTransaction transaction, long stripId, DefectInferenceResult result)
        {
            const string sql = "INSERT INTO inspection_defect(strip_id, camera_type, defect_index, defect_class, probability, probability_margin, measured_value_um, spec_value_um, ai_judgement, judgement_reason, ref_image_path, def_image_path)" +
                               "VALUES(strip_id, camera_type, defect_index, defect_class, probability, probability_margin, measured_value_um, spec_value_um, ai_judgement, judgement_reason, ref_image_path, def_image_path);";
            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("strip_id", stripId);
                command.Parameters.AddWithValue("camera_type",result.CameraType.ToString());
                command.Parameters.AddWithValue("defect_index",result.DefectIndex);
                command.Parameters.AddWithValue("defect_class",result.DefectClass.ToString());
                command.Parameters.AddWithValue("probability",result.ClassificationProbability);
                command.Parameters.AddWithValue("probability_margin",result.ClassificationMargin);
                command.Parameters.AddWithValue("measured_value_um",result.MeasuredValueUm);
                command.Parameters.AddWithValue("spec_value_um",result.SpecValueUm);
                command.Parameters.AddWithValue("ai_judgement",result.Judgement.ToString());
                command.Parameters.AddWithValue("judgement_reason",result.JudgementReason ?? "");

                /*
                 * 아직 Result에 개별 ImagePath가 없으니
                 * 일단 NULL.
                 *
                 * Review 구현하면서
                 * 별도 이미지 저장 경로를 넣자.
                 */
                command.Parameters.AddWithValue("ref_image_path",DBNull.Value);
                command.Parameters.AddWithValue("def_image_path",DBNull.Value);

                command.ExecuteNonQuery();
            }
        }
        #endregion

        #region Create Table.

        private const string CreateLotTableSql =
@"
CREATE TABLE IF NOT EXISTS inspection_lot
(
    id BIGINT NOT NULL AUTO_INCREMENT,

    device_name VARCHAR(100) NOT NULL,
    product_name VARCHAR(200) NOT NULL,
    order_number VARCHAR(100) NOT NULL,

    start_time DATETIME(3) NOT NULL,
    end_time DATETIME(3) NULL,

    created_at DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),

    PRIMARY KEY (id),

    INDEX idx_lot_order
    (
        device_name,
        product_name,
        order_number
    )
)
ENGINE=InnoDB
DEFAULT CHARSET=utf8mb4;
";

        private const string CreateStripTableSql =
@"
CREATE TABLE IF NOT EXISTS inspection_strip
(
    id BIGINT NOT NULL AUTO_INCREMENT,

    lot_id BIGINT NOT NULL,

    strip_number INT NOT NULL,

    total_count INT NOT NULL DEFAULT 0,
    ok_count INT NOT NULL DEFAULT 0,
    ng_count INT NOT NULL DEFAULT 0,
    unknown_count INT NOT NULL DEFAULT 0,

    processing_time_ms BIGINT NOT NULL DEFAULT 0,

    inspected_at DATETIME(3) NOT NULL,

    created_at DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),

    PRIMARY KEY (id),

    UNIQUE KEY uq_lot_strip
    (
        lot_id,
        strip_number
    ),

    INDEX idx_strip_time
    (
        inspected_at
    ),

    CONSTRAINT fk_strip_lot
        FOREIGN KEY (lot_id)
        REFERENCES inspection_lot(id)
        ON DELETE CASCADE
)
ENGINE=InnoDB
DEFAULT CHARSET=utf8mb4;
";

        private const string CreateDefectTableSql =
@"
CREATE TABLE IF NOT EXISTS inspection_defect
(
    id BIGINT NOT NULL AUTO_INCREMENT,

    strip_id BIGINT NOT NULL,

    camera_type VARCHAR(20) NOT NULL,

    defect_index INT NOT NULL,

    defect_class VARCHAR(50) NULL,

    probability DOUBLE NOT NULL DEFAULT 0,
    probability_margin DOUBLE NOT NULL DEFAULT 0,

    measured_value_um DOUBLE NOT NULL DEFAULT 0,
    spec_value_um DOUBLE NOT NULL DEFAULT 0,

    ai_judgement VARCHAR(20) NOT NULL,

    judgement_reason VARCHAR(500) NULL,

    ref_image_path VARCHAR(1000) NULL,
    def_image_path VARCHAR(1000) NULL,

    created_at DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),

    PRIMARY KEY (id),

    UNIQUE KEY uq_strip_camera_defect
    (
        strip_id,
        camera_type,
        defect_index
    ),

    INDEX idx_defect_judgement
    (
        ai_judgement
    ),

    INDEX idx_defect_class
    (
        defect_class
    ),

    CONSTRAINT fk_defect_strip
        FOREIGN KEY (strip_id)
        REFERENCES inspection_strip(id)
        ON DELETE CASCADE
)
ENGINE=InnoDB
DEFAULT CHARSET=utf8mb4;
";
        #endregion
    }
}
