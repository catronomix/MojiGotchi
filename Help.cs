namespace MojiGotchi;

public class Help : Modal
{
	// help text
	private string _helpText = @"
DOEL:
Houd je virtuele huisdier in leven door zijn statistieken te beheren.

BESTURING:
- Pijltjes Omhoog/Omlaag: Navigeer door het menu.
- Enter: Bevestig je selectie.
- Escape: Sluit dit venster.

ACTIES:
- Voeden: Verhoogt verzadiging en energie. Pas op voor overvoeding!
- Spelen: Verhoogt humeur, maar kost energie.
- Aaien: Verhoogt humeur en maakt je pet een beetje moe.
- Wekken: Maakt je pet wakker als hij slaapt, maar dit schaadt zijn humeur.
- Slaap: Je pet valt vanzelf in slaap als hij moe is.";


    public Help() : base("MojiGotchi - Handleiding", Color.DarkGreen, Color.Green)
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
