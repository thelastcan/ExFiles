using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using Microsoft.VisualBasic;
using System.IO;


namespace Teste2
{


    public partial class Form1 : Form
    {


        static int CountChars(string value)
        {
            int result = 0;
            bool lastWasSpace = false;

            foreach (char c in value)
            {
                if (char.IsWhiteSpace(c))
                {
                    // A.
                    // Only count sequential spaces one time.
                    if (lastWasSpace == false)
                    {
                        result++;
                    }
                    lastWasSpace = true;
                }
                else
                {
                    // B.
                    // Count other characters every time.
                    result++;
                    lastWasSpace = false;
                }
            }
            return result;
        }


        public string WorkingDirectory = "/";
        public FluentFTP.FtpClient FtpConnection;

        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            ServerFilesTree.Nodes.Clear();
            FtpConnection = new FluentFTP.FtpClient(HostNameTextBox.Text);
            FtpConnection.Encoding = Encoding.UTF8;
            FtpConnection.ConnectTimeout = 10000;
            if (UsernameTextBox.Text != "")
            {
                FtpConnection.Credentials = new NetworkCredential(UsernameTextBox.Text, PasswordTextBox.Text);
            }
            try
            {
                FtpConnection.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Connecting");
                MessageBox.Show("Error: \n " + ex.Message);
                return;
            }
            MessageBox.Show("Conected");
            WorkingDirectory = "/";
            ServerFilesTree.Nodes.Add("/");
            ServerFilesTree.Nodes[0].Tag = "/";
            foreach (FluentFTP.FtpListItem Item in FtpConnection.GetListing(WorkingDirectory))
            {
                ServerFilesTree.Nodes[0].Nodes.Add(Item.FullName.Replace(WorkingDirectory,""));
                ServerFilesTree.Nodes[0].LastNode.Tag = Item.FullName;
                //MessageBox.Show("Item Name: " + ServerFilesTree.Nodes[0].LastNode.Text);
                //MessageBox.Show("Item Tag: " + ServerFilesTree.Nodes[0].LastNode.Tag);
            }
        }

        private void ServerFilesTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Boolean check = false;
            WorkingDirectory = ServerFilesTree.SelectedNode.Tag.ToString();
            foreach (FluentFTP.FtpListItem Item in FtpConnection.GetListing(WorkingDirectory))
            {
                foreach(TreeNode TestNode in ServerFilesTree.SelectedNode.Nodes)
                {
                    if (TestNode.Text == Item.FullName.Replace(WorkingDirectory + "/", ""))
                    { 
                       check = true;
                       break;
                    }
                    if (TestNode.Text == Item.FullName.Replace(WorkingDirectory, ""))
                    {
                        check = true;
                        break;
                    }
                }
                if (!check)
                {
                    ServerFilesTree.SelectedNode.Nodes.Add(Item.FullName.Replace(WorkingDirectory + "/", ""));
                    ServerFilesTree.SelectedNode.LastNode.Tag = Item.FullName;
                    //MessageBox.Show("Item Name: " + ServerFilesTree.SelectedNode.LastNode.Text);
                    //MessageBox.Show("Item Tag: " + ServerFilesTree.SelectedNode.LastNode.Tag);
                }
                check = false;

            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
                FtpConnection.Disconnect();
            ServerFilesTree.Nodes.Clear();
        }

        private void ServerFilesTree_DragDrop(object sender, DragEventArgs e)
        {
            MessageBox.Show("Dragging File");
        }

        private void Connection_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ServerFilesTree.ContextMenuStrip = contextMenuStrip1;

            DriveInfo[] alldrives = DriveInfo.GetDrives();
            var i = 0;
            foreach (DriveInfo info in alldrives)
            {
                ClientFilesTree.Nodes.Add(info.Name);
                ClientFilesTree.Nodes[i].Tag = info.Name;
                MessageBox.Show("Dive Name: " + ClientFilesTree.Nodes[i].Text + "Tag: " + ClientFilesTree.Nodes[i].Tag.ToString());
                i = i + 1;
            }

        }

        private void ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (ServerFilesTree.Nodes.Count > 0)
            {

            }
            else
            {
                SendKeys.Send("{ESC}");
            }
        }

        private void NewFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var FolderToCreate = Interaction.InputBox("Folder Name", "New Folder");
            try
            {
                FtpConnection.SetWorkingDirectory(ServerFilesTree.SelectedNode.Tag.ToString());
                FtpConnection.CreateDirectory(FolderToCreate);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Creating Folder " +  ex.Message);
                return;
            }
            finally
            {
                ServerFilesTree.SelectedNode.Nodes.Add(FolderToCreate);
                ServerFilesTree.SelectedNode.LastNode.Tag = ServerFilesTree.SelectedNode.Tag.ToString()  + "/" + FolderToCreate;
                ServerFilesTree.SelectedNode.Expand();
            }
        }

        private void DeleteFolderFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ServerFilesTree.SelectedNode.Text.Contains('.'))
            {
                try
                {
                    FtpConnection.DeleteFile(ServerFilesTree.SelectedNode.Tag.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Deleting File: " + ex.Message);
                    return;
                }
                finally
                {
                    ServerFilesTree.SelectedNode.Remove();
                }
            }
            else
            {
                try
                {
                    FtpConnection.DeleteDirectory(ServerFilesTree.SelectedNode.Tag.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Deleting Folder: " + ex.Message);
                    return;
                }
                finally
                {
                    ServerFilesTree.SelectedNode.Remove();
                }
            }
            
        }

        private void ClientFilesTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            bool checkVar = false;
            try
            {
                foreach (String DirectoryToadd in Directory.GetDirectories(ClientFilesTree.SelectedNode.Tag.ToString()))
                {
                    foreach(TreeNode NodeToCheck in ClientFilesTree.SelectedNode.Nodes)
                    {
                        if (DirectoryToadd.Replace(ClientFilesTree.SelectedNode.Tag.ToString(), "") == NodeToCheck.Text)
                        {
                            checkVar = true;
                            break;
                        }
                        if (DirectoryToadd.Replace(ClientFilesTree.SelectedNode.Tag.ToString() + @"\", "") == NodeToCheck.Text)
                        {
                            checkVar = true;
                            break;
                        }
                    }
                    if (!checkVar)
                    {
                        if (DirectoryToadd.Replace(ClientFilesTree.SelectedNode.Tag.ToString(), "").Contains(@"\"))
                        {
                            ClientFilesTree.SelectedNode.Nodes.Add(DirectoryToadd.Replace(ClientFilesTree.SelectedNode.Tag.ToString() + @"\", ""));
                        }
                        else
                        {
                            ClientFilesTree.SelectedNode.Nodes.Add(DirectoryToadd.Replace(ClientFilesTree.SelectedNode.Tag.ToString(), ""));
                        }
                        ClientFilesTree.SelectedNode.LastNode.Tag = DirectoryToadd;
                    }
                    checkVar = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro Accessing Directory");
            }
        }
    }
}
/*
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using Microsoft.VisualBasic;

namespace Teste2
{


    public partial class Form1 : Form
    {


        static int CountChars(string value)
        {
            int result = 0;
            bool lastWasSpace = false;

            foreach (char c in value)
            {
                if (char.IsWhiteSpace(c))
                {
                    // A.
                    // Only count sequential spaces one time.
                    if (lastWasSpace == false)
                    {
                        result++;
                    }
                    lastWasSpace = true;
                }
                else
                {
                    // B.
                    // Count other characters every time.
                    result++;
                    lastWasSpace = false;
                }
            }
            return result;
        }


        public string WorkingDirectory = "/";
        public FluentFTP.FtpClient FtpConnection;

        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            ServerFilesTree.Nodes.Clear();
            FtpConnection = new FluentFTP.FtpClient(HostNameTextBox.Text);
            FtpConnection.Encoding = Encoding.UTF8;
            FtpConnection.ConnectTimeout = 10000;
            if (UsernameTextBox.Text != "")
            {
                FtpConnection.Credentials = new NetworkCredential(UsernameTextBox.Text, PasswordTextBox.Text);
            }
            try
            {
                FtpConnection.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Connecting");
                MessageBox.Show("Error: \n " + ex.Message);
                return;
            }
            MessageBox.Show("Conected");
            WorkingDirectory = "/";
            ServerFilesTree.Nodes.Add("/");
            ServerFilesTree.Nodes[0].Tag = "/";
            foreach (FluentFTP.FtpListItem Item in FtpConnection.GetListing(WorkingDirectory))
            {
                ServerFilesTree.Nodes[0].Nodes.Add(Item.FullName.Replace(WorkingDirectory, ""));
                ServerFilesTree.Nodes[0].LastNode.Tag = Item.FullName;
                //MessageBox.Show("Item Name: " + ServerFilesTree.Nodes[0].LastNode.Text);
                //MessageBox.Show("Item Tag: " + ServerFilesTree.Nodes[0].LastNode.Tag);
            }
        }

        private void ServerFilesTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Boolean check = false;
            WorkingDirectory = ServerFilesTree.SelectedNode.Tag.ToString();
            foreach (FluentFTP.FtpListItem Item in FtpConnection.GetListing(WorkingDirectory))
            {
                foreach (TreeNode TestNode in ServerFilesTree.SelectedNode.Nodes)
                {
                    if (TestNode.Text == Item.FullName.Replace(WorkingDirectory + "/", ""))
                    {
                        check = true;
                        break;
                    }
                    if (TestNode.Text == Item.FullName.Replace(WorkingDirectory, ""))
                    {
                        check = true;
                        break;
                    }
                }
                if (!check)
                {
                    ServerFilesTree.SelectedNode.Nodes.Add(Item.FullName.Replace(WorkingDirectory + "/", ""));
                    ServerFilesTree.SelectedNode.LastNode.Tag = Item.FullName;
                    //MessageBox.Show("Item Name: " + ServerFilesTree.SelectedNode.LastNode.Text);
                    //MessageBox.Show("Item Tag: " + ServerFilesTree.SelectedNode.LastNode.Tag);
                }
                check = false;

            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            FtpConnection.Disconnect();
            ServerFilesTree.Nodes.Clear();
        }

        private void ServerFilesTree_DragDrop(object sender, DragEventArgs e)
        {
            MessageBox.Show("Dragging File");
        }

        private void Connection_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ServerFilesTree.ContextMenuStrip = contextMenuStrip1;
        }

        private void ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (ServerFilesTree.Nodes.Count > 0)
            {

            }
            else
            {
                SendKeys.Send("{ESC}");
            }
        }

        private void NewFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var FolderToCreate = Interaction.InputBox("Folder Name", "New Folder");
            try
            {
                FtpConnection.SetWorkingDirectory(ServerFilesTree.SelectedNode.Tag.ToString());
                FtpConnection.CreateDirectory(FolderToCreate);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Creating Folder " + ex.Message);
                return;
            }
            finally
            {
                ServerFilesTree.SelectedNode.Nodes.Add(FolderToCreate);
                ServerFilesTree.SelectedNode.LastNode.Tag = ServerFilesTree.SelectedNode.Tag.ToString() + "/" + FolderToCreate;
                ServerFilesTree.SelectedNode.Expand();
            }
        }

        private void DeleteFolderFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ServerFilesTree.SelectedNode.Text.Contains('.'))
            {
                try
                {
                    FtpConnection.DeleteFile(ServerFilesTree.SelectedNode.Tag.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Deleting File: " + ex.Message);
                    return;
                }
                finally
                {
                    ServerFilesTree.SelectedNode.Remove();
                }
            }
            else
            {
                try
                {
                    FtpConnection.DeleteDirectory(ServerFilesTree.SelectedNode.Tag.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Deleting Folder: " + ex.Message);
                    return;
                }
                finally
                {
                    ServerFilesTree.SelectedNode.Remove();
                }
            }

        }
    }
}
*/