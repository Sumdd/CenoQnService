using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CenoQnService
{
    public class m_cFfmpeg
    {
        public static bool m_fInToOut(string m_sIn, string m_sOut, bool m_bIsWaitForExit = false, string m_sAc = "", string m_sCmdStr = "")
        {
            try
            {
                string m_sCmd = $"-y -i \"{m_sIn}\"{{ac}}\"{m_sOut}\"";
                if (m_sIn.IndexOf(".pcm", StringComparison.OrdinalIgnoreCase) > 0)
                    m_sCmd = $"-y -f s16be -ac 1 -ar 8000 -acodec pcm_s16le -i \"{m_sIn}\" \"{m_sOut}\"";
                else
                {
                    switch (m_sAc)
                    {
                        case "1":
                            m_sCmd = m_sCmd.Replace("{ac}", " -ac 1 ");
                            break;
                        case "2":
                            m_sCmd = m_sCmd.Replace("{ac}", " -ac 2 ");
                            break;
                        default:
                            m_sCmd = m_sCmd.Replace("{ac}", " ");
                            break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(m_sCmdStr))
                    m_sCmd = m_sCmdStr.Replace("[in]", m_sIn).Replace("[out]", m_sOut);

                return m_fUse(m_sCmd, m_bIsWaitForExit);
            }
            catch (Exception ex)
            {
                Log.Instance.Success($"[AutoxRecToTxt][m_cFfmpeg][m_fUse][{ex.Message}]");
                return false;
            }
        }

        private static bool m_fUse(string m_sCmd, bool m_bIsWaitForExit, bool m_bLog = false)
        {
            string executablePath = Path.Combine(m_cFfmpeg.m_fWhere, "ffmpeg.exe");

            var info = new ProcessStartInfo();
            info.FileName = string.Format("\"{0}\"", executablePath);
            info.WorkingDirectory = m_cFfmpeg.m_fWhere;
            info.Arguments = m_sCmd;

            info.RedirectStandardInput = false;
            info.RedirectStandardOutput = false;
            info.RedirectStandardError = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            string m_sData = "\r\n";

            using (var proc = new Process())
            {
                proc.StartInfo = info;
                proc.Start();
                proc.ErrorDataReceived += (a, b) =>
                {
                    if (b != null && b.Data != null)
                    {
                        m_sData += $"{b.Data}\r\n";
                    }
                };
                proc.BeginErrorReadLine();
                if (m_bIsWaitForExit)
                {
                    proc.WaitForExit(1000 * 15);
                }
                else
                {
                    proc.WaitForExit();
                }
            }

            if (m_bLog)
            {

                if (m_sData.Contains($"Output #0"))
                {
                    Log.Instance.Success(m_sData);
                    return true;
                }
                else
                {
                    Log.Instance.Error(m_sData);
                    return false;
                }
            }
            else
            {
                if (!m_sData.Contains($"Output #0")) Log.Instance.Error(m_sData);
                return true;
            }
        }

        private static string m_fWhere
        {
            get
            {
                return System.AppDomain.CurrentDomain.BaseDirectory;
            }
        }
    }
}
