using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGLDemo
{
    public class Game : GameWindow
    {
        private readonly float[] vertices =
        {
            // Position
            -0.5f, -0.5f, -0.5f, // Front face
             0.5f, -0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,

            -0.5f, -0.5f,  0.5f, // Back face
             0.5f, -0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f,

            -0.5f,  0.5f,  0.5f, // Left face
            -0.5f,  0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,

             0.5f,  0.5f,  0.5f, // Right face
             0.5f,  0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,

            -0.5f, -0.5f, -0.5f, // Bottom face
             0.5f, -0.5f, -0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f, -0.5f,

            -0.5f,  0.5f, -0.5f, // Top face
             0.5f,  0.5f, -0.5f,
             0.5f,  0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f, -0.5f
        };

        // indices for triangles
        private readonly uint[] indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private readonly Vector3 lightPos = new Vector3(1.2f, 1.0f, 2.0f);
        private int vaoModel;
        private int vaoLamp;

        private Shader? lampShader;
        private Shader? lightingShader;


        // Identifiers for vertex and element buffers
        private int elementBufferObject;
        private int vertexBufferObject;
        private int vertexArrayObject;

        // Objects for shaders and textures
        private Shader? shader;
        private Texture? texture;
        private Texture? secondTexture;


        private Camera? camera;
        private bool firstMove = true;
        private Vector2 lastPos;
        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            // Set window color
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // Activate tester for Z-fighting
            GL.Enable(EnableCap.DepthTest);

            // Create and bind objects
            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);


            lightingShader = new Shader("Shaders/shader.vert", "Shaders/lighting.frag");
            lampShader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

            {
                vaoModel = GL.GenVertexArray();
                GL.BindVertexArray(vaoModel);

                var vertexLoc = lightingShader.GetAttribLocation("aPos");
                GL.EnableVertexAttribArray(vertexLoc);
                GL.VertexAttribPointer(vertexLoc, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            }

            {
                vaoLamp = GL.GenVertexArray();
                GL.BindVertexArray(vaoLamp);

                // Set the vertex attributes (only position data for our lamp)
                var vertexLoc = lampShader.GetAttribLocation("aPos");
                GL.EnableVertexAttribArray(vertexLoc);
                GL.VertexAttribPointer(vertexLoc, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            }

            camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

            CursorState = CursorState.Grabbed;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(vaoModel);

            lightingShader!.Use();

            lightingShader.SetMatrix4("model", Matrix4.Identity);
            lightingShader.SetMatrix4("view", camera!.GetViewMatrix());
            lightingShader.SetMatrix4("projection", camera.GetProjectionMatrix());

            lightingShader.SetVector3("objectColor", new Vector3(1.0f, 0.5f, 1.0f));
            lightingShader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.BindVertexArray(vaoLamp);

            lampShader!.Use();

            Matrix4 lampMatrix = Matrix4.CreateScale(0.2f);
            lampMatrix = lampMatrix * Matrix4.CreateTranslation(lightPos);

            lampShader.SetMatrix4("model", lampMatrix);
            lampShader.SetMatrix4("view", camera.GetViewMatrix());
            lampShader.SetMatrix4("projection", camera.GetProjectionMatrix());

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused)
            {
                return;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Keys.W))
            {
                camera!.Position += camera.Front * cameraSpeed * (float)e.Time; // Forward
            }

            if (input.IsKeyDown(Keys.S))
            {
                camera!.Position -= camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                camera!.Position -= camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                camera!.Position += camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                camera!.Position += camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                camera!.Position -= camera.Up * cameraSpeed * (float)e.Time; // Down
            }

            // Get the mouse state
            var mouse = MouseState;

            if (firstMove) // This bool variable is initially set to true.
            {
                lastPos = new Vector2(mouse.X, mouse.Y);
                firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - lastPos.X;
                var deltaY = mouse.Y - lastPos.Y;
                lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                camera!.Yaw += deltaX * sensitivity;
                camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            camera!.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);

            camera!.AspectRatio = Size.X / (float)Size.Y;
        }
    }
}
