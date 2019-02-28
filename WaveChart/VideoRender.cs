using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace WaveChart
{
    public class WaveFormToDraw
    {
        public WaveFormToDraw(List<short> _shortData, int _offset_x)
        {
            ShortData = _shortData;
            OffsetX = _offset_x;
        }

        public List<short> ShortData { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
    }

    internal static class VideoRender
    {
        public static float globalOffsetX = 0;
        public static float globalOffsetScaleX = 0.005f;
        public static float globalOffsetY = 0;
        public static float globalOffsetScaleY = 0.005f;
        public static float globalZoomX = 1;
        public static float globalZoomScaleX = 0.005f;

        
        public static void DrawTrianglesForSingleWaveform(IEnumerable<short> fixed_sound_data)
        {
            
        }
        
        public static void DrawTrianglesForWaveformList(IEnumerable<WaveFormToDraw> waveFormList)
        {
            var counter = 0;
            foreach (var item in waveFormList)
            {
                item.OffsetY = counter;
                counter--;
            }
            
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

//                        if (globalOffsetX > 0)
//                        {
//                            globalOffsetX = 0;
//                        }
                        if (!game.Keyboard[Key.X])
                        {
                            globalOffsetX = game.Mouse.GetCursorState().Scroll.X * globalOffsetScaleX * -1;
//                          globalOffsetY = game.Mouse.GetCursorState().Scroll.Y * globalOffsetScaleY * -1;
                        }
                        else
                        {
                            globalZoomX = game.Mouse.GetCursorState().Scroll.Y * globalZoomScaleX;
                        }



//                       
                        Console.WriteLine( game.Mouse.XDelta + " " + game.Mouse.Y + " " + 
                                           game.Keyboard[Key.Right] + " " + game.Mouse.GetCursorState());
                    };
    
                    game.RenderFrame += (sender, e) =>
                    {
                        // render graphics
                        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    
                        GL.MatrixMode(MatrixMode.Projection);
                        GL.LoadIdentity();
                        GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);


                        var offset = -1f + globalOffsetX;
                        var someXCoefficient = 0.003f * globalZoomX;//0.01f;
                        var triangleHeight = 0.00003f;

                        var waveFormItemYOffsetCfcnt = 0.6f;
                        
                        var _globalOffsetY = globalOffsetY + 1f - waveFormItemYOffsetCfcnt;

                        foreach (var item in waveFormList)
                        {
                            var points = GetPointsFromAudio(item.ShortData);
            
                            points = points.Take(1000).ToList();
//                item.Offset;
//                item.ShortData;

                            foreach (var pointItem in points)
                            {
                                GL.Begin(PrimitiveType.Triangles);
                                GL.Color3(Color.MidnightBlue);
                                GL.Vertex2(offset + 0.0f + 
                                           someXCoefficient * pointItem.x + someXCoefficient * item.OffsetX,
                                    _globalOffsetY + waveFormItemYOffsetCfcnt * item.OffsetY);
                                GL.Color3(Color.SpringGreen);
    //                            GL.Color3(Color.Yellow);
                                GL.Vertex2(offset + someXCoefficient + 
                                           someXCoefficient * pointItem.x + someXCoefficient * item.OffsetX,
                                    _globalOffsetY + waveFormItemYOffsetCfcnt * item.OffsetY);
                                GL.Color3(Color.Red);
                                GL.Vertex2(offset + someXCoefficient + 
                                           someXCoefficient * pointItem.x + someXCoefficient * item.OffsetX,
                                    _globalOffsetY + triangleHeight * pointItem.y + waveFormItemYOffsetCfcnt * item.OffsetY);
                                GL.End();
                            }
                        }
                        game.SwapBuffers();
                    };
    
//                    MouseState current, previous;
// 
//                    void UpdateMouse()
//                    {
//                        current = Mouse.GetState();
//                        if (current != previous)
//                        {
//                            // Состояние мыши изменилос
//                            int xdelta = current.X - previous.X;
//                            int ydelta = current.Y - previous.Y;
//                            int zdelta = current.Wheel - previous.Wheel;
//                        }
//                        previous = current;
//                    }

 
                   
                    
                    // Run the game at 60 updates per second
                    game.Run(120.0); //60
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            
        }

        [STAThread]
        public static void DrawTriangles_old(IEnumerable<short> fixed_sound_data)
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