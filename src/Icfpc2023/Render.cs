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
Shift                                  - faster movement
Q/E                                   - zoom
LMB/RMB                          - show/hide connections for musician
hold MMB                         - show coords
U                                       - submit solutions
Esc                                     - close app
L                                        - toggle legend";
    private Mutex mut = new Mutex();
    private View mainView = new View(new Vector2f(0, 0), new Vector2f(1024f, 768f));
    private View hudView = new View(new Vector2f(0, 0), new Vector2f(1024f, 768f));
    private RectangleShape room = new RectangleShape();
    private RectangleShape stage = new RectangleShape();
    private List<CircleShape> attendees = new List<CircleShape>();
    private List<CircleShape> musicians = new List<CircleShape>();
    private Api.Problem? _problem;
    private Dictionary<int,VertexArray> connections = new Dictionary<int,VertexArray>();

    public void setProblem(Api.Problem problem)
    {
        mut.WaitOne();
        _problem = problem;
        room.FillColor = Color.Magenta;
        room.Size = new Vector2f((float)problem.RoomWidth, (float)problem.RoomHeight);
        stage.FillColor = Color.Red;
        stage.Size = new Vector2f((float)problem.StageWidth, (float)problem.StageHeight);
        stage.Position = new Vector2f((float)problem.StageBottomLeft.ElementAt(0), (float)problem.StageBottomLeft.ElementAt(1));
        attendees.Clear();
        foreach (var attendee in problem.Attendees)
        {
            var cir = new CircleShape(1.0f);
            cir.FillColor = Color.White;
            cir.Position = new Vector2f((float)attendee.X - cir.Radius, (float)attendee.Y - cir.Radius); 
            attendees.Add(cir);
        }
        mut.ReleaseMutex();
    }
    public void setSolution(Api.Placements placements)
    {
        mut.WaitOne();
        musicians.Clear();
        for (var i = 0; i < placements.PlacementsList.Count; ++i)
        {
            var cir = new CircleShape(5.0f);
            var instrument = _problem.Musicians.ElementAt(i);
            cir.FillColor = new Color((byte)(30 * ((instrument / 54) % 9)),
                                      (byte)(30 * ((instrument / 6) % 9)),
                                      (byte)(105 + 30 * (instrument % 6)),
                                        255);
            cir.Position = new Vector2f((float)placements.PlacementsList[i].X - cir.Radius,
                                        (float)placements.PlacementsList[i].Y - cir.Radius); 
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
        bool showCoords = false;

        var window = new RenderWindow(new VideoMode(1024 , 768), "icfpc2023");
        window.SetFramerateLimit(60);
        
        using Font openSans = new("./assets/fonts/OpenSans-Regular.ttf");
        using Text helpLabel = new(HelpMsg, openSans, 10);
        helpLabel.Position = new Vector2f(20, 0);
        using Text coordsLabel = new("", openSans, 14);

        window.Resized += (sender, e) =>
        {
            mainView.Reset(new FloatRect(new Vector2f(0, 0),
                                         new Vector2f(e.Width, e.Height)));
            hudView.Reset(new FloatRect(new Vector2f(0, 0),
                                         new Vector2f(e.Width, e.Height)));
        };
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
        window.MouseButtonReleased += (sender, e) =>
        {
            if (e.Button == Mouse.Button.Middle)
            {
                showCoords = false;
            }
        };
        window.MouseButtonPressed += (sender, e) =>
        {
            if (e.Button == Mouse.Button.Left || e.Button == Mouse.Button.Right)
            {
                window.SetView(mainView);
                var coords = window.MapPixelToCoords(new Vector2i(e.X, e.Y));
                for (var i = 0; i < musicians.Count; ++i)
                {
                    var musician = musicians.ElementAt(i);
                    if (coords.X > musician.Position.X && coords.X < musician.Position.X + 2 * musician.Radius &&
                        coords.Y > musician.Position.Y && coords.Y < musician.Position.Y + 2 * musician.Radius)
                    {
                        if (e.Button == Mouse.Button.Right)
                        {
                            connections.Remove(i);
                            return;
                        }
                        var musicianConnections = new VertexArray(PrimitiveType.Lines);
                        if (!connections.TryAdd(i, musicianConnections))
                        {
                            return;
                        }
                        var instrument = _problem.Musicians.ElementAt(i);
                        var musicianVertex = new Vertex(new Vector2f(musician.Position.X + musician.Radius,
                                                                    musician.Position.Y + musician.Radius));
                        foreach (var attendee in _problem.Attendees)
                        {
                            musicianConnections.Append(musicianVertex);
                            musicianConnections.Append(new Vertex(new Vector2f((float)attendee.X, (float)attendee.Y)));
                        }
                        return;
                    }
                }
            }
            if (e.Button == Mouse.Button.Middle)
            {
                window.SetView(mainView);
                var coords = window.MapPixelToCoords(new Vector2i(e.X, e.Y));
                coordsLabel.Position = new Vector2f(e.X + 20, e.Y);
                coordsLabel.DisplayedString = "(" + coords.X.ToString() + " | " + coords.Y.ToString() + ")";
                showCoords = true;
            }
        };
        while (window.IsOpen)
        {
            mut.WaitOne();
            window.DispatchEvents();
            window.Clear();
            mainView.Move(movement * shift);
            mainView.Zoom(zoom);
            window.SetView(mainView);
            window.Draw(room);
            window.Draw(stage);
            foreach (var musicianConnections in connections)
            {
                window.Draw(musicianConnections.Value);
            }
            foreach (var attendee in attendees)
            {
                window.Draw(attendee); 
            }
            foreach (var musician in musicians)
            {
                window.Draw(musician); 
            }
            window.SetView(hudView);
            if (legend)
            {
                window.Draw(helpLabel);

            }
            if (showCoords)
            {
                window.Draw(coordsLabel);
            }
            window.Display();
            mut.ReleaseMutex();
        }
    }
}