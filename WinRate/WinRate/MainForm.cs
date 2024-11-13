using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinRate
{
    public partial class MainForm : Form
    {
        private string _selectedFilePath;
        private Dictionary<string, Dictionary<string, int>> _battleData = new Dictionary<string, Dictionary<string, int>>();

        public MainForm()
        {
            InitializeComponent();

            Initialize();
        }

        #region Event:
        private void buttonLoadDeck_Click(object sender, EventArgs e)
        {
            LockForm();

            // 顯示打開文件對話框
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Choose a deck file";
                openFileDialog.Filter = "Text file (*.txt)|*.txt";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this._selectedFilePath = openFileDialog.FileName;

                    //初始化資料
                    Initialize();

                    // 擷取文件名稱
                    string fileName = Path.GetFileNameWithoutExtension(this._selectedFilePath);

                    // 更新標籤顯示文件名稱
                    buttonLoadDeck.Text = $"{fileName}";

                    //改字型
                    foreach (char c in buttonLoadDeck.Text)
                    {
                        if (IsChinese(c))
                        {
                            buttonLoadDeck.Font = new Font("Microsoft JhengHei", 16, FontStyle.Bold); // 中文字體
                        }
                        else
                        {
                            buttonLoadDeck.Font = new Font("Segoe Print", 16, FontStyle.Bold); // 英文字體
                        }
                    }

                    try
                    {
                        // 使用 StreamReader 逐行讀取文件內容
                        using (StreamReader reader = new StreamReader(this._selectedFilePath))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                //時間-輸贏-對手
                                string[] tmp = line.Split(',');

                                this._battleData["All"]["Matches"] += 1;
                                this._battleData[tmp[2]]["Matches"] += 1;

                                switch (tmp[1])
                                {
                                    case "W":
                                        this._battleData["All"]["Win"] += 1;
                                        this._battleData[tmp[2]]["Win"] += 1;
                                        break;
                                    case "L":
                                        this._battleData["All"]["Lose"] += 1;
                                        this._battleData[tmp[2]]["Lose"] += 1;
                                        break;
                                    case "D":
                                        this._battleData["All"]["Draw"] += 1;
                                        this._battleData[tmp[2]]["Draw"] += 1;
                                        break;
                                }
                                
                            }
                        }

                        buttonWin.Enabled = true;
                        buttonLose.Enabled = true;
                        buttonDraw.Enabled = true;
                        buttonDelete.Enabled = true;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"File error: {ex.Message}", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Initialize();
                    }

                    //
                    UpdateInfo();
                }
            }

            UnlockForm();
        }

        private void buttonWin_Click(object sender, EventArgs e)
        {
            LockForm();

            string element = GetElement();

            if (element != null)
            {
                try
                {
                    File.AppendAllText(this._selectedFilePath, DateTime.Now.ToString("yyyyMMdd") + ",W," + element + "," + textBoxMemo.Text + Environment.NewLine);
                    textBoxMemo.Text = "";

                    //
                    this._battleData["All"]["Matches"] += 1;
                    this._battleData["All"]["Win"] += 1;
                    this._battleData[element]["Matches"] += 1;
                    this._battleData[element]["Win"] += 1;

                    //
                    UpdateInfo();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Win error: {ex.Message}", "Message", MessageBoxButtons.OK);
                }
            }
            else
            {
                MessageBox.Show("Please choose one opponent！", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            UnlockForm();
        }

        private void buttonLose_Click(object sender, EventArgs e)
        {
            LockForm();

            string element = GetElement();

            if (element != null)
            {
                try
                {
                    File.AppendAllText(this._selectedFilePath, DateTime.Now.ToString("yyyyMMdd") + ",L," + element + "," + textBoxMemo.Text + Environment.NewLine);
                    textBoxMemo.Text = "";

                    //
                    this._battleData["All"]["Matches"] += 1;
                    this._battleData["All"]["Lose"] += 1;
                    this._battleData[element]["Matches"] += 1;
                    this._battleData[element]["Lose"] += 1;

                    //
                    UpdateInfo();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lose error: {ex.Message}", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please choose one opponent！", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            UnlockForm();
        }

        private void buttonDraw_Click(object sender, EventArgs e)
        {
            LockForm();

            string element = GetElement();

            if (element != null)
            {
                try
                {
                    File.AppendAllText(this._selectedFilePath, DateTime.Now.ToString("yyyyMMdd") + ",D," + element + "," + textBoxMemo.Text + Environment.NewLine);
                    textBoxMemo.Text = "";

                    //
                    this._battleData["All"]["Matches"] += 1;
                    this._battleData["All"]["Draw"] += 1;
                    this._battleData[element]["Matches"] += 1;
                    this._battleData[element]["Draw"] += 1;

                    //
                    UpdateInfo();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Draw error: {ex.Message}", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please choose one opponent！", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            UnlockForm();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            LockForm();

            // 讀取所有行
            var lines = File.ReadAllLines(this._selectedFilePath).ToList();

            if (lines.Count > 0)
            {
                // 顯示最後一行內容並詢問是否刪除
                DialogResult result = MessageBox.Show(
                    $"Last record：{lines.Last()}\n\nDelete confirm？",
                    "Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                // 根據用戶的選擇來執行
                if (result == DialogResult.Yes)
                {
                    //還原數據
                    //時間-輸贏-對手-備註
                    string[] tmp = lines.Last().Split(',');

                    this._battleData["All"]["Matches"] -= 1;
                    this._battleData[tmp[2]]["Matches"] -= 1;

                    switch (tmp[1])
                    {
                        case "W":
                            this._battleData["All"]["Win"] -= 1;
                            this._battleData[tmp[2]]["Win"] -= 1;
                            break;
                        case "L":
                            this._battleData["All"]["Lose"] -= 1;
                            this._battleData[tmp[2]]["Lose"] -= 1;
                            break;
                        case "D":
                            this._battleData["All"]["Draw"] -= 1;
                            this._battleData[tmp[2]]["Draw"] -= 1;
                            break;
                    }
                    //

                    // 刪除最後一行
                    lines.RemoveAt(lines.Count - 1);

                    // 將更新後的內容寫回檔案
                    File.WriteAllLines(this._selectedFilePath, lines);

                    //
                    UpdateInfo();
                }
            }

            UnlockForm();
        }
        #endregion

        #region Function:
        private void LockForm()
        {
            this.Enabled = false; // 鎖定表單
        }

        private void UnlockForm()
        {
            this.Enabled = true; // 解鎖表單
        }

        private void Initialize()
        {
            buttonLoadDeck.Text = "Load Deck";
            textBoxMemo.Text = "";

            labelWRGrass.Text = "- %\n0/0/0";
            labelWRFire.Text = "- %\n0/0/0";
            labelWRWater.Text = "- %\n0/0/0";
            labelWRLighting.Text = "- %\n0/0/0";
            labelWRPhychic.Text = "- %\n0/0/0";
            labelWRFighting.Text = "- %\n0/0/0";
            labelWRDarkness.Text = "- %\n0/0/0";
            labelWRMetal.Text = "- %\n0/0/0";
            labelWRDragon.Text = "- %\n0/0/0";
            labelWRNormal.Text = "- %\n0/0/0";

            labelWR.Text = "- %";
            labelT.Text = "0";
            labelWin.Text = "0";
            labelLose.Text = "0";
            labelDraw.Text = "0";


            this._battleData = new Dictionary<string, Dictionary<string, int>>
            {
                { "All", CreateBattleStats() },
                { "Grass", CreateBattleStats() },
                { "Fire", CreateBattleStats() },
                { "Water", CreateBattleStats() },
                { "Lighting", CreateBattleStats() },
                { "Phychic", CreateBattleStats() },
                { "Fighting", CreateBattleStats() },
                { "Darkness", CreateBattleStats() },
                { "Metal", CreateBattleStats() },
                { "Dragon", CreateBattleStats() },
                { "Normal", CreateBattleStats() }
            };

        }

        private Dictionary<string, int> CreateBattleStats()
        {
            return new Dictionary<string, int>
            {
                { "Matches", 0 },
                { "Win", 0 },
                { "Lose", 0 },
                { "Draw", 0 }
            };
        }

        private string GetElement()
        {
            if (radioButtonGrass.Checked)
            {
                return "Grass";
            }
            else if (radioButtonFire.Checked)
            {
                return "Fire";
            }
            else if (radioButtonWater.Checked)
            {
                return "Water";
            }
            else if (radioButtonLighting.Checked)
            {
                return "Lighting";
            }
            else if (radioButtonPhychic.Checked)
            {
                return "Phychic";
            }
            else if (radioButtonFighting.Checked)
            {
                return "Fighting";
            }
            else if (radioButtonDarkness.Checked)
            {
                return "Darkness";
            }
            else if (radioButtonMetal.Checked)
            {
                return "Metal";
            }
            else if (radioButtonDragon.Checked)
            {
                return "Dragon";
            }
            else if (radioButtonNormal.Checked)
            {
                return "Normal";
            }

            return null;
        }

        private void UpdateInfo()
        {
            //All
            labelWR.Text = this._battleData["All"]["Matches"] > 0 ? ((double)this._battleData["All"]["Win"] / this._battleData["All"]["Matches"] * 100).ToString("F2") + " %" : "- %";
            labelT.Text = this._battleData["All"]["Matches"].ToString();
            labelWin.Text = this._battleData["All"]["Win"].ToString();
            labelLose.Text = this._battleData["All"]["Lose"].ToString();
            labelDraw.Text = this._battleData["All"]["Draw"].ToString();

            //
            labelWRGrass.Text = this._battleData["Grass"]["Matches"] > 0 ? ((double)this._battleData["Grass"]["Win"] / this._battleData["Grass"]["Matches"] * 100).ToString("F2") + " %" : "- %";
            labelWRFire.Text = this._battleData["Fire"]["Matches"] > 0 ? ((double)this._battleData["Fire"]["Win"] / this._battleData["Fire"]["Matches"] * 100).ToString("F2") + " %" : "- %";
            labelWRWater.Text = this._battleData["Water"]["Matches"] > 0 ? ((double)this._battleData["Water"]["Win"] / this._battleData["Water"]["Matches"] * 100).ToString("F2") + " %" : "- %";
            labelWRLighting.Text = this._battleData["Lighting"]["Matches"] > 0 ? ((double)this._battleData["Lighting"]["Win"] / this._battleData["Lighting"]["Matches"] * 100).ToString("F2") + " %" : "- %";
            labelWRPhychic.Text = this._battleData["Phychic"]["Matches"] > 0 ? ((double)this._battleData["Phychic"]["Win"] / this._battleData["Phychic"]["Matches"] * 100).ToString("F2") + " %" : "- %";
            labelWRFighting.Text = this._battleData["Fighting"]["Matches"] > 0 ? ((double)this._battleData["Fighting"]["Win"] / this._battleData["Fighting"]["Matches"] * 100).ToString("F2") + " %" : "- %";
            labelWRDarkness.Text = this._battleData["Darkness"]["Matches"] > 0 ? ((double)this._battleData["Darkness"]["Win"] / this._battleData["Darkness"]["Matches"] * 100).ToString("F2") + " %" : "- %";
            labelWRMetal.Text = this._battleData["Metal"]["Matches"] > 0 ? ((double)this._battleData["Metal"]["Win"] / this._battleData["Metal"]["Matches"] * 100).ToString("F2") + " %" : "- %";
            labelWRDragon.Text = this._battleData["Dragon"]["Matches"] > 0 ? ((double)this._battleData["Dragon"]["Win"] / this._battleData["Dragon"]["Matches"] * 100).ToString("F2") + " %" : "- %";
            labelWRNormal.Text = this._battleData["Normal"]["Matches"] > 0 ? ((double)this._battleData["Normal"]["Win"] / this._battleData["Normal"]["Matches"] * 100).ToString("F2") + " %" : "- %";

            //
            labelWRGrass.Text += "\n" + this._battleData["Grass"]["Matches"] + "/" + this._battleData["Grass"]["Win"] + "/" + this._battleData["Grass"]["Lose"] + "/" + this._battleData["Grass"]["Draw"];
            labelWRFire.Text += "\n" + this._battleData["Fire"]["Matches"] + "/" + this._battleData["Fire"]["Win"] + "/" + this._battleData["Fire"]["Lose"] + "/" + this._battleData["Fire"]["Draw"];
            labelWRWater.Text += "\n" + this._battleData["Water"]["Matches"] + "/" + this._battleData["Water"]["Win"] + "/" + this._battleData["Water"]["Lose"] + "/" + this._battleData["Water"]["Draw"];
            labelWRLighting.Text += "\n" + this._battleData["Lighting"]["Matches"] + "/" + this._battleData["Lighting"]["Win"] + "/" + this._battleData["Lighting"]["Lose"] + "/" + this._battleData["Lighting"]["Draw"];
            labelWRPhychic.Text += "\n" + this._battleData["Phychic"]["Matches"] + "/" + this._battleData["Phychic"]["Win"] + "/" + this._battleData["Phychic"]["Lose"] + "/" + this._battleData["Phychic"]["Draw"];
            labelWRFighting.Text += "\n" + this._battleData["Fighting"]["Matches"] + "/" + this._battleData["Fighting"]["Win"] + "/" + this._battleData["Fighting"]["Lose"] + "/" + this._battleData["Fighting"]["Draw"];
            labelWRDarkness.Text += "\n" + this._battleData["Darkness"]["Matches"] + "/" + this._battleData["Darkness"]["Win"] + "/" + this._battleData["Darkness"]["Lose"] + "/" + this._battleData["Darkness"]["Draw"];
            labelWRMetal.Text += "\n" + this._battleData["Metal"]["Matches"] + "/" + this._battleData["Metal"]["Win"] + "/" + this._battleData["Metal"]["Lose"] + "/" + this._battleData["Metal"]["Draw"];
            labelWRDragon.Text += "\n" + this._battleData["Dragon"]["Matches"] + "/" + this._battleData["Dragon"]["Win"] + "/" + this._battleData["Dragon"]["Lose"] + "/" + this._battleData["Dragon"]["Draw"];
            labelWRNormal.Text += "\n" + this._battleData["Normal"]["Matches"] + "/" + this._battleData["Normal"]["Win"] + "/" + this._battleData["Normal"]["Lose"] + "/" + this._battleData["Normal"]["Draw"];
        }

        private bool IsChinese(char c)
        {
            return c >= 0x4E00 && c <= 0x9FFF;
        }
        #endregion

    }
}
