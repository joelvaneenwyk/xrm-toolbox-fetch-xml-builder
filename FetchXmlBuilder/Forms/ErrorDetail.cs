﻿using Microsoft.Xrm.Sdk;
using System;
using System.ServiceModel;
using System.Windows.Forms;

namespace Cinteros.Xrm.FetchXmlBuilder.Forms
{
    public partial class ErrorDetail : Form
    {
        private DateTime timestamp;

        public static void ShowDialog(Control owner, Exception error, string heading = null)
        {
            if (error == null)
            {
                return;
            }
            new ErrorDetail(error, heading).ShowDialog(owner);
        }

        private ErrorDetail(Exception error, string heading)
        {
            timestamp = DateTime.Now;
            InitializeComponent();
            if (!string.IsNullOrEmpty(heading))
            {
                Text = heading;
            }
            AddErrorInfo(error);
            Height = 200;
        }

        private void AddErrorInfo(Exception error)
        {
            try
            {
                var msg = error.Message;
                if (error is FaultException<OrganizationServiceFault> srcexc)
                {
                    msg = srcexc.Message;
                    if (srcexc.Detail is OrganizationServiceFault orgerr)
                    {
                        msg = orgerr.Message;
                        if (orgerr.InnerFault != null)
                        {
                            msg = orgerr.InnerFault.Message;
                        }
                        txtErrorCode.Text = "0x" + orgerr.ErrorCode.ToString("X");
                    }
                }
                if (msg.StartsWith("System") && msg.Contains(": ") && msg.Split(':')[0].Length < 50)
                {
                    msg = msg.Substring(msg.IndexOf(':') + 1);
                }
                if (msg.Contains("MetadataCacheDetails: "))
                {
                    msg = msg.Substring(0, msg.IndexOf("MetadataCacheDetails"));
                }
                if (msg.Contains("   at "))
                {
                    msg = msg.Substring(0, msg.IndexOf("   at "));
                }
                msg = msg.Trim();
                txtInfo.Text = msg;
                txtException.Text = error.GetType().ToString();
                txtMessage.Text = error.Message;
                txtCallStack.Text = error.StackTrace.Trim();
            }
            catch
            {
                txtInfo.Text = error.Message;
            }
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            if (Height < 300)
            {
                Height = 550;
            }
            else
            {
                Height = 200;
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            var details = "Error Time: " + timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\n";
            details += txtException.Text;
            if (!string.IsNullOrEmpty(txtErrorCode.Text))
            {
                details += $" ({txtErrorCode.Text})";
            }
            details += $"\n{txtMessage.Text}";
            details += $"\n{txtCallStack.Text}";
            Clipboard.SetText(details);
            MessageBox.Show("Copied all details.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
