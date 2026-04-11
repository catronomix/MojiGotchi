namespace MojiGotchi;

public class HighScores : Modal
{
	public HighScores() : base(LM.Get("modaltitle_highscores"), Color.DarkMagenta, Color.Red)
	{

	}

	public void UpdatePage(Vec2 size)
	{
		SetSpriteBg(size);
		ClearContentSprite(size);

		//get high scores
		List<string[]> highScores = DataManager.GetHighScores();

		//sort by age from oldest to youngest
		highScores.Sort((a, b) =>
		{
			double ageA = double.TryParse(a[1], out double resA) ? resA : 0;
			double ageB = double.TryParse(b[1], out double resB) ? resB : 0;
			return ageB.CompareTo(ageA);
		});

		//turn high scores into sprite
		for (int i = 0; i < Math.Min(highScores.Count, size.Y - 5); i++)
		{
			string name = highScores[i][0];
			double ageSeconds = double.TryParse(highScores[i][1], out double res) ? res : 0;
			string ageDisplay = DataManager.GetAgeString(TimeSpan.FromSeconds(ageSeconds));

			int maxNameWidth = 20;
			string scoreLine = $"{(i + 1).ToString().PadLeft(2)}. {name.PadRight(maxNameWidth)} {ageDisplay}";
			Sprite lineSprite = new Sprite(new Vec2(scoreLine.Length, 1));

			for (int x = 0; x < scoreLine.Length; x++)
			{
				// write aligned horizontally
				lineSprite.WriteCell(new Vec2(x, 0), new ScreenCell 
				{ 
					Character = scoreLine[x], 
					Color = Color.White, 
					BgColor = this.BgColor
				});
			}
			AddContent(lineSprite, new Vec2(4, i + 4));
		}
	}
}
