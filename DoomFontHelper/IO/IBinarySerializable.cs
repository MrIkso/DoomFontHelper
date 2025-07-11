namespace DoomFontHelper.Helpers.IO
{
    public interface IBinarySerializable
    {
        public void Load(BinaryReader br);

        public void Save(BinaryWriter bw);
    }
}
