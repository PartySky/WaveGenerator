using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace WaveChart
{
    internal static class VideoRender
    {
        [STAThread]
        public static void DrawTriangles(IEnumerable<short> fixed_sound_data)
        {
            var points = GetPointsFromAudio(fixed_sound_data);
            
            points = points.Take(1000).ToList();
            try
            {
                Console.WriteLine("");
                using (var game = new GameWindow())
                {
                    game.Load += (sender, e) =>
                    {
                        // setup settings, load textures, sounds
                        game.VSync = VSyncMode.Off;
                    };
    
                    game.Resize += (sender, e) =>
                    {
                        GL.Viewport(0, 0, game.Width, game.Height);
                    };
    
                    game.UpdateFrame += (sender, e) =>
                    {
                        // add game logic, input handling
                        if (game.Keyboard[Key.Escape])
                        {
                            game.Exit();
                        }
                    };
    
                    game.RenderFrame += (sender, e) =>
                    {
                        // render graphics
                        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    
                        GL.MatrixMode(MatrixMode.Projection);
                        GL.LoadIdentity();
                        GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);


                        var offset = -1f;
//                        var offset = -0.95f;

                        var someXCoefficient = 0.01f;
//                        var someXCoefficient = 0.002f;
                        
                        foreach (var item in points)
                        {
                            GL.Begin(PrimitiveType.Triangles);
                            GL.Color3(Color.MidnightBlue);
                            GL.Vertex2(offset + 0.0f + someXCoefficient * item.x, 0.0f);
                            GL.Color3(Color.SpringGreen);
//                            GL.Color3(Color.Yellow);
                            GL.Vertex2(offset + someXCoefficient + someXCoefficient * item.x, 0.0f);
                            GL.Color3(Color.Red);
                            GL.Vertex2(offset + someXCoefficient + someXCoefficient * item.x, 0.0001f * item.y);
                            GL.End();
                        }
                        
                        if (false)
                        {
//                            GL.Begin(PrimitiveType.Triangles);
//        
//                            GL.Color3(Color.MidnightBlue);
//                            GL.Vertex2(-1.0f, 1.0f);
//                            GL.Color3(Color.SpringGreen);
//                            GL.Vertex2(0.0f, -1.0f);
//                            GL.Color3(Color.Ivory);
//                            GL.Vertex2(1.0f, 1.0f);
//        
//                            GL.End();
                            
                            
                            GL.Begin(PrimitiveType.Triangles);

                            GL.Color3(Color.MidnightBlue);
                            GL.Vertex2(0.0f, 0.0f);
                            GL.Color3(Color.SpringGreen);
                            GL.Vertex2(0.5f, 0.0f);
                            GL.Color3(Color.Red);
                            GL.Vertex2(0.5f, 0.5f);
        
                            GL.End();
                        }

                        if (false)
                        {
                            GL.Begin(PrimitiveType.Points);
                            GL.Enable(EnableCap.ProgramPointSize);
//                            GL.PointSize(2205);
//                            GL.Color3(Color.Red);
//                            GL.Vertex3(0, 0, 0);
//                            GL.Color3(Color.Green);
//                            GL.Vertex3(0, 0, 0);
//                            GL.Vertex2(1.0f, 1.0f);
//                            GL.DrawArrays(PrimitiveType.Points, 0, 1);
                            GL.End();
                        }

                        game.SwapBuffers();
                    };
    
                    // Run the game at 60 updates per second
                    game.Run(6.0); //60
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private static List<Point> GetPointsFromAudio(IEnumerable<short> fixedSoundData)
        {
            var points = new List<Point>();
            var counter = 0;
            foreach (var item in fixedSoundData)
            {
                points.Add(new Point
                {
                    x = counter, 
                    y = item
                });
                counter++;
            }
            return points;
        }
    }
}