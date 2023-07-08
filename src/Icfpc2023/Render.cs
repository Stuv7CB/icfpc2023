using System.IO;
using System.Collections.Generic;
using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace Icfpc2023;
public class Render
{
    private const string HelpMsg = 
@"
WASD/Arrows                     - movement
Shift                           - faster movement
Q/E                             - zoom
U                                 - submit solutions
Esc                             - close app
L                               - toggle legend";
    private Mutex mut = new Mutex();
    private View firstView = new View(new Vector2f(1024f, 768f), new Vector2f(512f, 384f));
    private RectangleShape room = new RectangleShape();
    private RectangleShape stage = new RectangleShape();
    private List<CircleShape> attendees = new List<CircleShape>();
    private List<CircleShape> musicians = new List<CircleShape>();

    public void setData(Api.Problem problem, Api.Placements placements)
    {
        mut.WaitOne();
        room.FillColor = Color.Green;
        room.Size = new Vector2f(problem.RoomWidth, problem.RoomHeight);
        stage.FillColor = Color.Red;
        stage.Size = new Vector2f(problem.StageWidth, problem.StageHeight);
        stage.Position = new Vector2f(problem.StageBottomLeft.ElementAt(0), problem.StageBottomLeft.ElementAt(1));
        foreach (var attendee in problem.Attendees)
        {
            var cir = new CircleShape(1.0f);
            cir.FillColor = Color.White;
            cir.Position = new Vector2f(attendee.X, attendee.Y); 
            attendees.Add(cir);
        }
        for (var i = 0; i < placements.PlacementsList.Count; ++i)
        {
            var cir = new CircleShape(5.0f);
            var instrument = problem.Musicians.ElementAt(i);
            cir.FillColor = new Color((byte)(30 * ((instrument / 54) % 9)),
                                      (byte)(30 * ((instrument / 6) % 9)),
                                      (byte)(105 + 30 * (instrument % 6)),
                                        255);
            cir.Position = new Vector2f(placements.PlacementsList[i].X, placements.PlacementsList[i].Y); 
            musicians.Add(cir);
        }
        mut.ReleaseMutex();
    }
    public void run()
    {
        bool ctrl = false;
        int shift = 1;
        bool submit = false;
        var movement = new Vector2f(0f, 0f);
        var zoom = 1f;
        bool legend = true;

        var window = new RenderWindow(new VideoMode(1024 , 768), "icfpc2023");
        window.SetFramerateLimit(60);
        
        using Font openSans = new("./assets/fonts/OpenSans-Regular.ttf");
        using Text helpLabel = new(HelpMsg, openSans, 10);
        helpLabel.Position = new Vector2f(20, 0);

        window.Closed += (sender, e) => window.Close();
        window.KeyReleased += (sender, e) =>
        {
            switch (e.Code)
            {
                case Keyboard.Key.LControl: ctrl = false; break;
                case Keyboard.Key.LShift: shift = 1; break;
                case Keyboard.Key.Up:
                case Keyboard.Key.W:
                case Keyboard.Key.Down:
                case Keyboard.Key.S: movement.Y = 0f; break;
                case Keyboard.Key.Right:
                case Keyboard.Key.D: 
                case Keyboard.Key.Left:
                case Keyboard.Key.A: movement.X = 0f; break;
                case Keyboard.Key.E: 
                case Keyboard.Key.Q: zoom = 1; break;
            }
        };
        window.KeyPressed += (sender, e) =>
        {
            switch (e.Code)
            {
                case Keyboard.Key.LControl: ctrl = true; break;
                case Keyboard.Key.LShift: shift = 10; break;
                case Keyboard.Key.Escape: window.Close(); break;
                case Keyboard.Key.U: submit = true; window.Close(); break;
                case Keyboard.Key.Up:
                case Keyboard.Key.W: movement.Y = -10f; break;
                case Keyboard.Key.Down:
                case Keyboard.Key.S: movement.Y = 10f; break;
                case Keyboard.Key.Right:
                case Keyboard.Key.D: movement.X = 10f; break;
                case Keyboard.Key.Left:
                case Keyboard.Key.A: movement.X = -10f; break;
                case Keyboard.Key.E: zoom = 0.95f; break;
                case Keyboard.Key.Q: zoom = 1.05f; break;
                case Keyboard.Key.L: legend^= true; break;
            }
            
        };
        var defaultView = window.DefaultView;
        while (window.IsOpen)
        {
            mut.WaitOne();
            window.DispatchEvents();
            window.Clear();
            firstView.Move(movement * shift);
            firstView.Zoom(zoom);
            window.SetView(firstView);
            window.Draw(room);
            window.Draw(stage);
            foreach (var attendee in attendees)
            {
                window.Draw(attendee); 
            }
            foreach (var musician in musicians)
            {
                window.Draw(musician); 
            }
            window.SetView(defaultView);
            if (legend)
            {
                window.Draw(helpLabel);
            }
            window.Display();
            mut.ReleaseMutex();
        }
    }
}