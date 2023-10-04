using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using FontStyle = System.Drawing.FontStyle;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Minensuchboot
{
    public partial class mainForm : Form
    {
        private Boolean clickFieldEnabled = true;
        private List<Vector> mines = new List<Vector>();
        private Dictionary<String, Block> blocks = new Dictionary<String, Block>();
        private int flagCount = 0;
        private int openCount = 0;
        private int openWinCount = 0;

        public mainForm()
        {
            InitializeComponent();
        }

        private class Block
        {
            public Vector xy { get; set; }
            public Boolean isMine { get; set; }
            public int count { get; set; }
            public Label label { get; set; }
            public Boolean clickable = true;
            public Boolean closed = true;

            public Block(Vector xy, Boolean isMine, Label label)
            {
                this.xy = xy;
                this.isMine = isMine;
                this.label = label;
            }

            public Block(Vector xy, int count, Label label)
            {
                this.xy = xy;
                this.count = count;
                this.isMine = false;
                this.label = label;
            }

            public int setFlag()
            {
                if (clickable && closed)
                {
                    clickable = !clickable;
                    label.Text = "P";
                    label.ForeColor = Color.Orange;
                    clickable = false;
                    return -1;
                }
                else if (!clickable && closed)
                {
                    clickable = !clickable;
                    label.ForeColor = Color.Gray;
                    return 1;
                }

                return 0;
            }

            public void open()
            {

                closed = false;

                label.BackColor = Color.White;
                label.ForeColor = Color.Black;

                if (isMine)
                {
                    label.Text = "X";
                }
                else if (count > 0)
                {
                    label.Text = count.ToString();
                }
                else {
                    label.ForeColor = label.BackColor;
                }
            }
        }

        private void generateMines(int amount, int max)
        {

            List<Vector> mines = new List<Vector>();
            Random randNum = new Random();

            while (amount > 0)
            {

                int x = randNum.Next(1, max);
                int y = randNum.Next(1, max);
                Vector xy = new Vector(x, y);
                Vector item = mines.Find(vec => vec.Equals(xy));

                if (item == new Vector(0, 0))
                {
                    mines.Add(xy);
                    amount -= 1;
                }
            }

            this.mines = mines;
        }

        private void generateField(int fieldSize, int minesAmount, int scaleSize, int margin, Size position)
        {

            generateMines(minesAmount, fieldSize);
            List<Vector> mines = this.mines;

            Font font = new Font("Arial", 10);

            for (int i = 1; i < fieldSize + 1; i++)
            {
                for (int j = 1; j < fieldSize + 1; j++)
                {

                    Label label = new Label
                    {
                        Location = new Point(position.Height + (scaleSize+margin) * (i-1), position.Width + (scaleSize+margin) * (j-1)),
                        Name = "block" + i.ToString() + ":" + j.ToString(),
                        Size = new Size(scaleSize, scaleSize),
                        Font = font,
                        TabIndex = 0,
                        BackColor = Color.Gray,
                        ForeColor = Color.Black,
                        TextAlign = ContentAlignment.MiddleCenter,

                    };

                    label.Click += new EventHandler(checkBlock);
                    Controls.Add(label);

                    Vector xy = new Vector(i, j);
                    Vector? item = mines.Find(e => e.Equals(xy));

                    if (item == new Vector(0, 0))
                    {
                        int minesCount = checkMines(xy);

                        Block block = new Block(xy, minesCount, label);
                        blocks[label.Name] = block;

                    }
                    else
                    {
                        Block block = new Block(xy, true, label);
                        blocks[label.Name] = block;
                    }
                }
            }

        }

        private void restartButton_Click(object sender, EventArgs e) 
        {
            Button button = sender as Button;
            String difficulty = button.Name;
            gameStart(difficulty);
        }

        private void menuButton_Click(object sender, EventArgs e) 
        {
            loadMainMenu();
        }

        private void gameUiLoad(String difficulty) 
        {
            Label flagCounter = new Label {
                Name = "flagCounter",
                Text = flagCount.ToString("D2"),
                Size = new Size(60, 30),
                Location = new Point(207, 20),
                Font = new Font("Arial", 18),
                BackColor = Color.White,
                ForeColor = Color.Green,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle,
            };
            Controls.Add(flagCounter);

            Button restartButton = new Button {
                Name = difficulty,
                Text = "Restart",
                Size = new Size(100, 32),
                Location = new Point(340, 20),
                Font = new Font(this.Font.FontFamily, 15, FontStyle.Bold),
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                
            };
            restartButton.Click += restartButton_Click;
            Controls.Add(restartButton);

            Button menuButton = new Button{
                Name = "menuButton",
                Text = "Menu",
                Size = new Size(100, 32),
                Location = new Point(40, 20),
                Font = new Font(this.Font.FontFamily, 15, FontStyle.Bold),
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            menuButton.Click += menuButton_Click;
            Controls.Add(menuButton);
        }

        private void gameStart(String difficulty) 
        {
            Controls.Clear();
            clickFieldEnabled = true;
            this.Size = new Size(500, 550);

            int fieldSize;
            int minesAmount;

            if (difficulty == "diff1")
            {
                fieldSize = 16;
                minesAmount = 40;
            }
            else if (difficulty == "diff2") 
            {
                fieldSize = 21;
                minesAmount = 80;
            }
            else
            {
                fieldSize = 9;
                minesAmount = 10;
            }

            flagCount = minesAmount;
            openWinCount = fieldSize^2;
            Size windowSize = this.Size;
            int fieldGeometrySize = (int)Math.Round(windowSize.Width * 0.8);
            int margin = 1;
            Size fieldPosition = new Size(
                (int)Math.Round(windowSize.Width * 0.15),
                (int)Math.Round(windowSize.Width * 0.08)
                );
            int fieldScaleSize = (fieldGeometrySize - (margin * fieldSize)) / fieldSize;


            generateField(fieldSize, minesAmount, fieldScaleSize, margin, fieldPosition);
            gameUiLoad(difficulty);
        }

        private void gameOver(bool isWin) 
        {
            clickFieldEnabled = false;

            if (isWin)
            {
                MessageBox.Show("You Win!");
            } else {
                MessageBox.Show("Game Over");
            }
        }

        private void loadMainMenu() 
        {
            Controls.Clear();
            this.Size = new Size(300, 250);

            Point initPos = new Point(40,20);
            Size size = new Size(200, 40);
            int distance = 10;

            List<String> difficulties = new List<String> {"Easy", "Medium", "Hard"};

            for (int i = 0; i <= 2; i++)
            {
                Point pos = new Point(initPos.X, initPos.Y+ i * (size.Height + distance));
                Button button = new Button {
                    Name = "diff" + i,
                    Text = difficulties[i],
                    Location = pos,
                    Size = size,
                };

                button.Click += difficultyButton_Click;
                Controls.Add(button);
            }

        }


        private void mainForm_Load(object sender, EventArgs e)
        {
            loadMainMenu();
        }


        private void difficultyButton_Click(object sender, EventArgs e) 
        {
            Button button = (Button)sender;
            gameStart(button.Name);

        }


        private int checkMines(Vector pos)
        {
            int count = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vector check_pos = new Vector(pos.X - 1 + i, pos.Y - 1 + j);
                    Vector? mine = mines.Find(k => k.Equals(check_pos));
                    if (mine != new Vector(0, 0))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private void flagCountUpdate(int sum) 
        {
            flagCount += sum;
            Label control = Controls.Find("flagCounter", false)[0] as Label;
            control.Text = flagCount.ToString("D2");
        }

        private void checkBlock(object sender, EventArgs e)
        {
            if (!clickFieldEnabled) return;

            Label label = (Label)sender;
            MouseEventArgs eventArgs = (MouseEventArgs)e;
            Block block = blocks[label.Name];

            if (eventArgs.Button == MouseButtons.Right)
            {

                flagCountUpdate(block.setFlag());
                return;
            }

            if (eventArgs.Button != MouseButtons.Left || !block.clickable) return;

            if (block.isMine)
            {
                foreach (Vector minePos in mines)
                {
                    String ctrlName = "block" + minePos.X + ":" + minePos.Y;
                    blocks[ctrlName].open();
                    clickFieldEnabled = false;
                }
                gameOver(false);
                return;

            }
            else if (!block.isMine)
            {
                openCount += 1;
                block.open();
            }
            
            if (block.count == 0)
            {
                openCount += 1;
                block.open();
                blockRecOpen(block);
            }
            if (flagCount == 0 && (openCount >= openWinCount))
            {
                gameOver(true);
            }
        }



        private void blockRecOpen(Block block) {

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    String ctrlName = "block" + (block.xy.X - 1 + i).ToString() + ":" + (block.xy.Y - 1 + j).ToString();
                    try
                    {

                        Block check_block = blocks[ctrlName];

                        if (!check_block.isMine)
                        {
                            if ((check_block.count == 0) && check_block.closed)
                            {
                                openCount += 1;
                                check_block.open();
                                blockRecOpen(check_block);
                            }
                            else if (check_block.closed)
                            {
                                openCount += 1;
                                check_block.open();
                            }
                        }
                        
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        private void hoverBlock(object sender, EventArgs e)
        {
            Label label = (sender as Label);
            label.BackColor = Color.White;
        }
    }
}
