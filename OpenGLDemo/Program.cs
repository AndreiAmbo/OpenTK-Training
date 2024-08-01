namespace OpenGLDemo
{
    public class Program
    {
        public static void Main()
        {
            using (Game game = new Game(800, 600, "LearnOpenTK"))
            {
                game.Run();
            }
        }
    }
}
