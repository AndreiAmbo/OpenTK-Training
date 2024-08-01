namespace OpenGLDemo
{
    public class Program
    {
        public static void Main()
        {
            using (Game game = new Game(1280, 768, "LearnOpenTK"))
            {
                game.Run();
            }
        }
    }
}
