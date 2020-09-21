namespace Arcsecond
{
    public class Many : Parser
    {
        public Many(Parser parser)
        {
            Transform = new ManyAtLeast(0, parser).Transform;
        }
    }
}
