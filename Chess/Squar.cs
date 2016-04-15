/***************************************************************
 * File: Squar.cs
 * Created By: Syed Ghulam Akbar		Date: 28 June, 2005
 * Description: A square class to draw the squares on the windows form.
 ***************************************************************/

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Speech.Synthesis;

namespace Chess 
{
	/// <summary>
	/// 
	/// </summary>
	public class Squar : PictureBox
	{
        SpeechSynthesizer sSynth = new SpeechSynthesizer();
        PromptBuilder style = new PromptBuilder();
                        
        private GameUI m_ParentGame;
		private static Image m_DraggedImage;	// Image being dragged
		private static Image m_ImageBeforeDrag;	// Image stored in square before dragging
		private static string m_DragSourceSquar;	// Square from which the drag begin
		private static string m_DragDestSquar;		// Square on which item is dropped

		public Squar(int row, int col, GameUI parentgame)
		{
            sSynth.Rate = -3;
            
            m_ParentGame = parentgame;

			// Initialize the square UI component
			if (parentgame!=null)
				Location = new System.Drawing.Point((row-1)*55+33, (col-1)*55+33);	// move the piece place holder to it's proper location
			else
				Location = new System.Drawing.Point((row-1)*55, (col-1)*55);
			Name = ""+(char)(row+64)+col;	// Generate unique name for the place holder
			Size = new System.Drawing.Size(55, 55);
			Visible = true;	
			SizeMode = PictureBoxSizeMode.CenterImage;

			if (parentgame!=null)
				InitializeComponent();
			//BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		}

		// Set the chess piece image
		public void DrawPiece(System.Drawing.Image pieceImage)
		{
			Image = pieceImage;
		}

		// Set the chess background square
		public void SetBackgroundSquar(Images ImageList)
		{
			int row=char.Parse(Name.Substring(0,1).ToUpper())-64; // Get row from first ascii char i.e. a=1, b=2 and so on
			int col=int.Parse(Name.Substring(1,1));				  // Get column value directly, as it's already numeric

			if (((row+col)%2==0)) // White cell
                BackgroundImage = ImageList["Black"];
			else
                BackgroundImage = ImageList["White"];
		}

		private void InitializeComponent()
		{
			// 
			// Square
			// 
			this.AllowDrop = true;
			this.Click += new System.EventHandler(this.Squar_Click);
			this.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.Squar_GiveFeedback);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Squar_DragEnter);
			this.DragLeave += new System.EventHandler(this.Squar_DragLeave);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Squar_DragDrop);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Squar_MouseDown);

		}

		private void Squar_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
            bool moved = false;

            // Allow the pieces to move by single click
            if (m_ParentGame.IsRunning && e.Button == MouseButtons.Left && !m_ParentGame.ChessGame.ActivePlay.IsComputer())
            {
                if (!string.IsNullOrEmpty(m_ParentGame.LastSelectedSquar) && this.Name != m_ParentGame.LastSelectedSquar)
                {
                    if (m_ParentGame.UserMove(m_ParentGame.LastSelectedSquar, this.Name))
                    {
                        moved = true;
                        m_ParentGame.LastSelectedSquar = "";
                    }
                }
            }

			if (this.Image != null && e.Button == MouseButtons.Left && !m_ParentGame.ChessGame.ActivePlay.IsComputer())	// square contains a piece
			{
				m_DraggedImage = this.Image;
				this.Image=null;
				m_DragSourceSquar=m_DragDestSquar=this.Name;	// get the source square being dragged
				this.DoDragDrop(this.Name, DragDropEffects.Move);

                m_ParentGame.LastSelectedSquar = m_DragDestSquar;

				if (m_DragSourceSquar==m_DragDestSquar) // No d&d performed
				{
                    if (moved == false)
                    {
                        m_ParentGame.Sounds.PlayClick();
                        m_ParentGame.SelectedSquar = m_DragSourceSquar;
                    }
                    else
                        m_ParentGame.SelectedSquar = "";

					m_ParentGame.RedrawBoard();
				}
				else
					m_ParentGame.UserMove(m_DragSourceSquar, m_DragDestSquar);
			}
		}

		private void Squar_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			// Current that square is not the square from where drag started
			m_ImageBeforeDrag = this.Image;	// store image in temporary variable
			this.Image = m_DraggedImage;
			e.Effect = DragDropEffects.Move;
		}

		private void Squar_GiveFeedback(object sender, System.Windows.Forms.GiveFeedbackEventArgs e)
		{
			e.UseDefaultCursors = false;
			Cursor.Current = Cursors.Hand;
		}

		private void Squar_DragLeave(object sender, System.EventArgs e)
		{
			this.Image=m_ImageBeforeDrag;
		}

		// Called when click on any chess square object
		private void Squar_Click(object sender, System.EventArgs e)
		{
			if (m_ParentGame.IsRunning && !m_ParentGame.ChessGame.ActivePlay.IsComputer())
			{
				Squar ChessSquar = (Squar)sender;
                // 				m_ParentGame.Sounds.PlayClick();
                style.AppendTextWithHint(ChessSquar.Name, SayAs.SpellOut);
                sSynth.Speak(style);  // audio output coordinates of clicked empty square
                style.ClearContent();

                m_ParentGame.SelectedSquar = ChessSquar.Name;
				m_ParentGame.RedrawBoard();
			}
		}

		private void Squar_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			m_DragDestSquar = this.Name;
            style.AppendTextWithHint(this.Name, SayAs.SpellOut);
            sSynth.Speak(style);  // audio output coordinates of clicked empty square
            style.ClearContent();
        }

	}
}
