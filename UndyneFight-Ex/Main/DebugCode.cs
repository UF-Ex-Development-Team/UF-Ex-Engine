namespace UndyneFight_Ex;
/// <summary>
/// Debug variables for the SDK
/// </summary>
public static class DebugState
{
	internal static bool[] ShieldAuto = [false, false, false, false];
	/// <summary>
	/// Show the cost of rendering on screen during a chart
	/// </summary>
#if DEBUG
	public const bool ShowRenderCost = true;
#else
    public const bool ShowRenderCost = false;
#endif
	/// <summary>
	/// Displays the intended hitbox of barrages during a chart
	/// </summary>
#if DEBUG
	public const bool ShowIntendedHitbox = false;
#else
    public const bool ShowIntendedHitbox = false;
#endif
	/// <summary>
	/// The version of UF-Ex
	/// </summary>
	public const string Version = "0.4.0";

}
internal partial class GameMain : Game
{
	private static void InstanceCode()
	{
		return;
		/*
            List<ChampionshipInfo> info = JsonSerializer.Deserialize<List<ChampionshipInfo>>(cur);
            ;
            Dictionary<string, Dictionary<string, ChampionshipParticipant>> existDivs = new();
            foreach (var i in info)
                foreach (var j in i.Participants)
                    if (!existDivs.ContainsKey(j.Value))
                        existDivs.Add(j.Value, new());

            foreach(var i in info)
                foreach(var j in i.Divisions.Values)
                {
                    if (!existDivs.ContainsKey(j.DivisionName)) continue;
                    ChampionshipScoreboard scoreboard = j.Scoreboard;
                    float[] accMax = new float[scoreboard.Members.First().AccuracyList.Length];
                    foreach(var obj in scoreboard.Members)
                    {
                        for(int k = 0; k < accMax.Length; k++)
                            accMax[k] = MathF.Max(accMax[k], obj.AccuracyList[k]);
                    }
                    foreach (var obj in scoreboard.Members)
                    {
                        ChampionshipParticipant p = new(obj.UUID, obj.Name, j);
                        p.AccuracyList = obj.AccuracyList;
                        for (int k = 0; k < accMax.Length; k++)
                        {
                            if (accMax[k] < 0.0001f) p.AccuracyList[k] = 0.998f;
                            else p.AccuracyList[k] = p.AccuracyList[k] / accMax[k];
                        }
                        if (!existDivs[j.DivisionName].ContainsKey(p.Name))
                            existDivs[j.DivisionName].Add(p.Name, p);
                        float v = existDivs[j.DivisionName][p.Name].Total;
                        if (p.Total > v)
                        {
                            existDivs[j.DivisionName][p.Name].AccuracyList = p.AccuracyList;
                            existDivs[j.DivisionName][p.Name].Update();
                        }
                    }
                }
            
            foreach(var dic in existDivs)
            {
                FileStream stream = new("Datas\\" + dic.Key + ".txt", FileMode.OpenOrCreate);
                StreamWriter textWriter = new StreamWriter(stream);
                
                foreach (var v in dic.Value) { 
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(v.Key.ToString() + " : ");
                    foreach (var c in v.Value.AccuracyList)
                    {
                        stringBuilder.Append(c + " ");
                    }
                    stringBuilder.Append(", Total = " + v.Value.Total);
                    textWriter.WriteLine(stringBuilder);
                }
                textWriter.Flush();
                textWriter.Dispose();
                stream.Dispose();
            }
            ;*/
		/* System.IO.FileStream stream = new System.IO.FileStream("Taster.txt", System.IO.FileMode.Open);

             List<byte> bytes = new List<byte>();
             while (stream.Position != stream.Length)
             {
                 bytes.Add((byte)stream.ReadByte()); 
             } byte[] res = bytes.ToArray();

             IO.IOEvent.WriteTmpFile("Taster.Tmpf", bytes);*/
		/*
             //var s = TEngine.Network.InformationLibrary.GetIP();
            //byte[] bytes = { 1, 2, 3, 4, 5, 6, 7 };
            //byte[] res = IO.IOEvent.Decoder(IO.IOEvent.Encoder(new List<byte>(bytes))).ToArray();
            //    var v= MathUtil.StringHash("Evelyne");
            //;
            // ChampionShips.LicenseMaker.MakeLicence(); 

            //  List<string> stringsOld = IO.IOEvent.ByteToString(IO.IOEvent.ReadTmpfFile("Datas\\tk"));
            //   stringsOld[240] = "Hard:score=0,AC=true,AP=true,Accuracy=0,mark=Failed";
            // List<string> stringsNew = IO.IOEvent.ByteToString(IO.IOEvent.ReadTmpfFile("Datas\\DJwwwNew"));
            //
            //strings.RemoveRange(100, 10);
            //   IO.IOEvent.WriteTmpFile("Datas\\tk", IO.IOEvent.StringToByte(stringsOld));
            */
	}
}