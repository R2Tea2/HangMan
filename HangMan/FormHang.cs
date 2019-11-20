using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HangMan
{
    public partial class FormHang : Form
    {
        /*
         * Developer            :- Gehan Fernando
         * Date                 :- 20th Dec 2013
         * 
         * Contact              :- +94772269625, f.gehan@gmail.com
         * 
         * Link                 :- http://en.wikipedia.org/wiki/Hangman_(game)
         * 
         * About                :- Hangman is a paper and pencil guessing game for two or more players. One player thinks of a word, 
         *                         phrase or sentence and the other tries to guess it by suggesting letters or numbers.
        */

        #region Variable List

        private StreamReader _reader = null;

        private String[] _wordListArray = null;
        private Char[] _wordArray = null;
        private Char[] _display = null;

        private Random _random = null;

        private String _randomWord = String.Empty;

        private int _wordIndex = 0;
        private int _previousNumber = 0;
        private int _imageIndex = 0;
        private int _remain = 0;
        private Boolean _isAbandon = false;

        private KeysConverter _converter = null;

        #endregion

        #region Form Events

        public FormHang()
        {
            InitializeComponent();
        }

        private void FormHang_Load(object sender, EventArgs e)
        {
            _random = new Random();
            _converter = new KeysConverter();

            this.LoadWordBox();
            this.Reset();
        }

        private void FormHang_KeyUp(object sender, KeyEventArgs e)
        {
            if (buttonNewGame.Enabled == true) return;
            string keyvalue = _converter.ConvertToString(e.KeyData);

            Char value = '@';
            Char.TryParse(keyvalue, out value);

            if (Char.IsLetter(value))
                this.SetGame(keyvalue);
        }

        private void FormHang_Shown(object sender, EventArgs e)
        {
            try
            {
                this.LoadWordList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonAbandon_Click(object sender, EventArgs e)
        {
            _isAbandon = true;

            this.GameStatus(true);
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            this.Reset();

            buttonAbandon.Enabled = false;
            buttonNewGame.Enabled = true;
        }

        private void buttonNewGame_Click(object sender, EventArgs e)
        {
            try
            {
                this.Reset();

                int showValues = 0;

                while (_previousNumber == _wordIndex )
                    _wordIndex = _random.Next(0, (_wordListArray.Length - 1));

                _previousNumber = _wordIndex;
                _randomWord = _wordListArray[_wordIndex].ToUpper();
                _wordArray = _randomWord.ToCharArray();
                
                this.CreateLabelBox(_wordArray.Length);

                foreach (Control control in this.Controls)
                    if (control is Button) control.Enabled = true;

                switch (_wordArray.Length)
                {
                    case 1:
                    case 2:
                    case 3:
                        showValues = 1;
                        break;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                        showValues = 2;
                        break;
                    default:
                        showValues = 3;
                        break;
                }

                Control.ControlCollection collection = this.Controls;
                _display = new Char[showValues];

                foreach (Control control in collection)
                {
                    if (control is Label)
                        if (control.Name != "labelAuthor")
                        {
                            switch (showValues)
                            {
                                case 1:
                                    if (control.Name == String.Format("{0}{1}", "labelWord", "01"))
                                    {
                                        control.Text = _wordArray[0].ToString();
                                        _display[0] = Char.Parse(control.Text);
                                    }
                                    break;
                                case 2:
                                    if (control.Name == String.Format("{0}{1}", "labelWord", "01"))
                                    {
                                        control.Text = _wordArray[0].ToString();
                                        _display[0] = Char.Parse(control.Text);
                                    }

                                    if (control.Name == String.Format("{0}{1}", "labelWord", (_wordArray.Length).ToString("00")))
                                    {
                                        control.Text = _wordArray[_wordArray.Length - 1].ToString();
                                        _display[1] = Char.Parse(control.Text);
                                    }
                                    break;
                                case 3:
                                    if (control.Name == String.Format("{0}{1}", "labelWord", "01"))
                                    {
                                        control.Text = _wordArray[0].ToString();
                                        _display[0] = Char.Parse(control.Text);
                                    }

                                    int middle = _wordArray.Length / 2;
                                    if (control.Name == String.Format("{0}{1}", "labelWord", middle.ToString("00")))
                                    {
                                        control.Text = _wordArray[middle - 1].ToString();
                                        _display[1] = Char.Parse(control.Text);
                                    }

                                    if (control.Name == String.Format("{0}{1}", "labelWord", (_wordArray.Length).ToString("00")))
                                    {
                                        control.Text = _wordArray[_wordArray.Length - 1].ToString();
                                        _display[2] = Char.Parse(control.Text);
                                    }
                                    break;
                            }
                        }
                }

                Array.Sort(_display);
                char value = '@';

                foreach (Char charvalue in _display)
                {
                    if (value == charvalue)continue;

                    value = charvalue;
                    int innerCount = (from c in _display
                                      where c == charvalue
                                      select c).Count();

                    int outerCount = (from c in _wordArray
                                      where c == charvalue
                                      select c).Count();


                    if (outerCount - innerCount <= 0)
                    {
                        foreach (Control control in collection)
                        {
                            if (control is Button)
                            {
                                if (control.Name == String.Format("{0}{1}", "button", value))
                                    control.Enabled = false;
                                break;
                            }
                        }
                    }
                }

                buttonAbandon.Enabled = true;
                buttonNewGame.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Game Methods

        private void Reset()
        {
            pictureBoxHangMan.Image = null;

            foreach (Control control in this.Controls)
            {
                if (control is Label)
                    if (control.Name != "labelAuthor")
                    {
                        control.ForeColor = Color.Lime;
                        control.Visible = false;
                        control.ResetText();
                    }

                if (control is Button) control.Enabled = false;
            }

            _wordIndex = 0;
            _imageIndex = 0;
            _isAbandon = false;
            _wordArray = null;

            buttonAbandon.Enabled = false;
        }

        private void ShowHangMan(int position)
        {
            switch (position)
            {
                case 1:
                    pictureBoxHangMan.Image = Properties.Resources._01;
                    break;
                case 2:
                    pictureBoxHangMan.Image = Properties.Resources._02;
                    break;
                case 3:
                    pictureBoxHangMan.Image = Properties.Resources._03;
                    break;
                case 4:
                    pictureBoxHangMan.Image = Properties.Resources._04;
                    break;
                case 5:
                    pictureBoxHangMan.Image = Properties.Resources._05;
                    break;
                case 6:
                    pictureBoxHangMan.Image = Properties.Resources._06;
                    break;
                case 7:
                    pictureBoxHangMan.Image = Properties.Resources._07;
                    break;
                case 8:
                    pictureBoxHangMan.Image = Properties.Resources._08;
                    break;
                case 9:
                    pictureBoxHangMan.Image = Properties.Resources._09;
                    break;
                default:
                    pictureBoxHangMan.Image = null;
                    break;
            }
        }

        private void TextButton(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            String btnText = btn.Text;

            this.SetGame(btnText);
        }

        private void LoadWordList()
        {
            _reader = new StreamReader(File.Open("WordList.txt", FileMode.Open, FileAccess.Read));

            int count = 0;
            _wordListArray = new String[1];

            while (_reader.Peek() != -1)
            {
                Application.DoEvents();

                count += 1;
                Array.Resize(ref _wordListArray, count);

                _wordListArray[count - 1] = _reader.ReadLine().Trim();
            }

            _reader.Close();
            _reader.Dispose();

            _reader = null;
        }

        private void LoadWordBox()
        {
            int lineOne = 198;
            int lineTwo = 198;

            for (int i = 1; i <= 18; i++)
            {
                String labelName = String.Format("{0}{1}", "labelWord", i.ToString("00"));
                Control ctrl = new Control();

                Label word = new Label();

                word.Name = String.Format("{0}{1}", "labelWord", i.ToString("00"));
                word.Size = new Size(35, 35);
                word.TextAlign = ContentAlignment.MiddleCenter;
                word.BackColor = Color.Black;
                word.ForeColor = Color.Lime;
                word.Font = new Font("Calibri", 20, FontStyle.Bold);

                if (i <= 9)
                {
                    word.Location = new Point(lineOne, 12);
                    lineOne += 37;
                }
                if (i >= 10)
                {
                    word.Location = new Point(lineTwo, 47);
                    lineTwo += 37;
                }

                word.Text = String.Empty;
                word.BorderStyle = BorderStyle.Fixed3D;
                word.Visible = true;

                this.Controls.Add(word);
            }
        }

        private void CreateLabelBox(int count)
        {
            int index = 1;

            Control.ControlCollection collection = this.Controls;

            foreach (Control control in collection)
                if (control is Label)
                    if (control.Name != "labelAuthor")
                        if (control.Name == String.Format("{0}{1}", "labelWord", index.ToString("00")))
                        {
                            control.Font = new Font("Calibri", 20, FontStyle.Bold);
                            control.ResetText();
                            control.Visible = true;

                            index += 1;
                            if (index > count) return;
                        }
        }

        private void GameStatus(Boolean islost)
        {
            if (islost) pictureBoxHangMan.Image = Properties.Resources._09;
            
            _wordIndex = 0;

            foreach (Control control in this.Controls)
            {
                if (control is Label)
                    if (control.Name != "labelAuthor")
                    {
                        if ((String.IsNullOrEmpty(control.Text)) && (control.Visible == true))
                        {
                            control.Font = new Font("Calibri", 20, FontStyle.Bold);
                            control.ForeColor = Color.DarkSeaGreen;
                            control.Text = _wordArray[_wordIndex].ToString();
                        }

                        if ((!String.IsNullOrEmpty(control.Text)) && (control.Visible == true)) _wordIndex += 1;
                    }

                if (control is Button) control.Enabled = false;
            }

            buttonAbandon.Enabled = false;
            buttonNewGame.Enabled = true;
        }

        private void SetGame(String word)
        {
            int result = (from w in _wordArray
                           where w == Char.Parse(word)
                           select w).Count();

            int count = 0;
            int[] indexArray = new int[1];

            for (int i = 0; i < _wordArray.Length; i++)
            {
                if (_wordArray[i] == Char.Parse(word))
                {
                    count += 1;
                    Array.Resize(ref indexArray, count);
                    indexArray[count - 1] = i;
                }
            }

            _remain = 0;
            Control.ControlCollection collection = this.Controls;

            foreach (Control control in collection)
            {
                if (control is Label)
                    if (control.Name != "labelAuthor")
                    {
                        foreach (int index in indexArray)
                        {
                            if (control.Name == String.Format("{0}{1}", "labelWord", (index + 1).ToString("00")) && control.Visible)
                            {
                                if (String.IsNullOrEmpty(control.Text))
                                    control.Text = word;
                            }
                        }

                        if (String.IsNullOrEmpty(control.Text) && control.Visible)
                            _remain += 1;
                    }

                if (control is Button)
                {
                    if (control.Name == String.Format("{0}{1}", "button", word.ToUpper()))
                    {
                        if (!control.Enabled) return;
                        control.Enabled = false;
                    }
                }
            }

            if (result == 0)
            {
                _imageIndex += 1;
                ShowHangMan(_imageIndex);
                if (_imageIndex >= 9) GameStatus(true);
                return;
            }

            if (_remain <= 0)
            {
                MessageBox.Show("Congratulations! You won.", 
                    this.Text, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                GameStatus(false);
            }
        }

        #endregion
    }
}