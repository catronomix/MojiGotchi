namespace MojiGotchi;

public class Help : Modal
{
	// help text
	private string _helpText = LM.Get("modaltext_help");


	public Help() : base(LM.Get("modaltitle_help"), Color.DarkGreen, Color.Green)
	{
		
	}

	public void UpdatePage(Vec2 size)
	{
		SetSpriteBg(size);
		ClearContentSprite(size);
		string[] lines = _helpText.Split('\n');

		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i].TrimEnd('\r');
			if (string.IsNullOrEmpty(line)) continue;

			Sprite lineSprite = new Sprite(new Vec2(line.Length, 1));
			for (int x = 0; x < line.Length; x++)
			{
				lineSprite.WriteCell(new Vec2(x, 0), new ScreenCell 
				{ 
					Character = line[x], 
					Color = Color.White, 
					BgColor = this.BgColor
				});
			}

			// Add content with a small margin (x=2) and vertical offset
			AddContent(lineSprite, new Vec2(4, i + 2));
		}
	}
}
