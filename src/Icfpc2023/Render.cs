using System.Threading.Channels;
using Icfpc2023.Api;
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
C                                - hide all connections 
hold MMB                         - show coords
U                                       - submit solutions
Esc                                     - close app
L                                        - toggle legend";

    private readonly object _lock = new object();
    private RenderWindow _window = new(new VideoMode(1024, 768), "icfpc2023");
    private View _mainView = new(new Vector2f(0, 0), new Vector2f(1024f, 768f));
    private View _hudView = new(new Vector2f(0, 0), new Vector2f(1024f, 768f));
    private Font _openSans = new("./assets/fonts/OpenSans-Regular.ttf");
    private Text _currentProblemLabel = new();
    private RectangleShape _room = new();
    private RectangleShape _stage = new();
    private List<CircleShape> _attendees = new();
    private List<CircleShape> _musicians = new();
    private List<CircleShape> _pillars = new();
    private Api.Problem? _problem;
    private int _problemId = 0;
    private int _totalProblemsCount = 0;
    private Dictionary<int, VertexArray> _connections = new();

    public Render(int totalProblemsCount)
    {
        _totalProblemsCount = totalProblemsCount;
        _window.SetFramerateLimit(60);
        _currentProblemLabel.Font = _openSans;
        _currentProblemLabel.Position = new(20, 0);
        _currentProblemLabel.CharacterSize = 14;
        _currentProblemLabel.Style = Text.Styles.Bold;
        _currentProblemLabel.DisplayedString = "?/" + _totalProblemsCount.ToString();
        _room.FillColor = Color.Magenta;
        _stage.FillColor = Color.Red;

    }

    public void SetProblem(Api.Problem problem, int problemId)
    {
        lock (_lock)
        {
            _connections.Clear();
            _problemId = problemId;
            _problem = problem;
            _currentProblemLabel.DisplayedString = _problemId.ToString() + "/" + _totalProblemsCount.ToString();
            _room.Size = new((float)problem.RoomWidth, (float)problem.RoomHeight);
            _stage.Size = new((float)problem.StageWidth, (float)problem.StageHeight);
            _stage.Position = new((float)problem.StageBottomLeft.ElementAt(0),
                (float)problem.StageBottomLeft.ElementAt(1));
            _attendees.Clear();
            foreach (var attendee in problem.Attendees)
            {
                var cir = new CircleShape(1.0f);
                cir.FillColor = Color.White;
                cir.Position = new((float)attendee.X - cir.Radius, (float)attendee.Y - cir.Radius);
                _attendees.Add(cir);
            }

            _pillars.Clear();
            foreach (var pillar in problem.Pillars)
            {
                var cir = new CircleShape((float)pillar.Radius);
                cir.FillColor = new(100, 100, 100, 255);
                cir.Position = new((float)pillar.Center.ElementAt(0) - cir.Radius,
                    (float)pillar.Center.ElementAt(1) - cir.Radius);
                _pillars.Add(cir);
            }
        }
    }

    public void SetSolution(Api.Placements placements)
    {
        lock (_lock)
        {
            _musicians.Clear();
            for (var i = 0; i < placements.PlacementsList.Count; ++i)
            {
                var cir = new CircleShape(5.0f);
                var instrument = _problem.Musicians.ElementAt(i);
                cir.FillColor = new((byte)(30 * ((instrument / 54) % 9)),
                    (byte)(30 * ((instrument / 6) % 9)),
                    (byte)(105 + 30 * (instrument % 6)),
                    255);
                cir.Position = new((float)placements.PlacementsList[i].X - cir.Radius,
                    (float)placements.PlacementsList[i].Y - cir.Radius);
                _musicians.Add(cir);
            }
        }
    }

    public void Run(ChannelReader<(int, Problem)> problemReader, ChannelReader<Placements> solutionReader)
    {
        var ctrl = false;
        var shift = 1;
        var submit = false;
        var movement = new Vector2f(0f, 0f);
        var zoom = 1f;
        var legend = true;
        var showCoords = false;

        using Font openSans = new("./assets/fonts/OpenSans-Regular.ttf");
        using Text helpLabel = new(HelpMsg, openSans, 10);
        helpLabel.Position = new Vector2f(20, 0);
        using Text coordsLabel = new("", openSans, 14);

        _window.Resized += (sender, e) =>
        {
            _mainView.Reset(new FloatRect(new Vector2f(0, 0),
                new Vector2f(e.Width, e.Height)));
            _hudView.Reset(new FloatRect(new Vector2f(0, 0),
                new Vector2f(e.Width, e.Height)));
        };
        _window.Closed += (sender, e) => _window.Close();
        _window.KeyReleased += (sender, e) =>
        {
            switch (e.Code)
            {
                case Keyboard.Key.LControl:
                    ctrl = false;
                    break;
                case Keyboard.Key.LShift:
                    shift = 1;
                    break;
                case Keyboard.Key.Up:
                case Keyboard.Key.W:
                case Keyboard.Key.Down:
                case Keyboard.Key.S:
                    movement.Y = 0f;
                    break;
                case Keyboard.Key.Right:
                case Keyboard.Key.D:
                case Keyboard.Key.Left:
                case Keyboard.Key.A:
                    movement.X = 0f;
                    break;
                case Keyboard.Key.E:
                case Keyboard.Key.Q:
                    zoom = 1;
                    break;
            }
        };
        _window.KeyPressed += (sender, e) =>
        {
            switch (e.Code)
            {
                case Keyboard.Key.LControl:
                    ctrl = true;
                    break;
                case Keyboard.Key.LShift:
                    shift = 10;
                    break;
                case Keyboard.Key.Escape:
                    _window.Close();
                    break;
                case Keyboard.Key.U:
                    submit = true;
                    _window.Close();
                    break;
                case Keyboard.Key.Up:
                case Keyboard.Key.W:
                    movement.Y = -10f;
                    break;
                case Keyboard.Key.Down:
                case Keyboard.Key.S:
                    movement.Y = 10f;
                    break;
                case Keyboard.Key.Right:
                case Keyboard.Key.D:
                    movement.X = 10f;
                    break;
                case Keyboard.Key.Left:
                case Keyboard.Key.A:
                    movement.X = -10f;
                    break;
                case Keyboard.Key.E:
                    zoom = 0.95f;
                    break;
                case Keyboard.Key.Q:
                    zoom = 1.05f;
                    break;
                case Keyboard.Key.L:
                    legend ^= true;
                    break;
                case Keyboard.Key.C:
                    _connections.Clear();
                    break;
            }

        };
        _window.MouseButtonReleased += (sender, e) =>
        {
            if (e.Button == Mouse.Button.Middle)
            {
                showCoords = false;
            }
        };
        _window.MouseButtonPressed += (sender, e) =>
        {
            if (e.Button == Mouse.Button.Left || e.Button == Mouse.Button.Right)
            {
                _window.SetView(_mainView);
                var coords = _window.MapPixelToCoords(new Vector2i(e.X, e.Y));
                for (var i = 0; i < _musicians.Count; ++i)
                {
                    var musician = _musicians.ElementAt(i);
                    if (coords.X > musician.Position.X && coords.X < musician.Position.X + 2 * musician.Radius &&
                        coords.Y > musician.Position.Y && coords.Y < musician.Position.Y + 2 * musician.Radius)
                    {
                        if (e.Button == Mouse.Button.Right)
                        {
                            _connections.Remove(i);
                            return;
                        }

                        var musicianConnections = new VertexArray(PrimitiveType.Lines);
                        if (!_connections.TryAdd(i, musicianConnections))
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
                _window.SetView(_mainView);
                var coords = _window.MapPixelToCoords(new Vector2i(e.X, e.Y));
                coordsLabel.Position = new(e.X + 20, e.Y);
                coordsLabel.DisplayedString = "(" + coords.X.ToString() + " | " + coords.Y.ToString() + ")";
                showCoords = true;
            }
        };
        while (_window.IsOpen)
        {
            if (problemReader.TryRead(out var problem))
            {
                SetProblem(problem.Item2, problem.Item1);
            }

            if (solutionReader.TryRead(out var solution))
            {
                SetSolution(solution);
            }

            lock (_lock)
            {
                _window.DispatchEvents();
                _window.Clear();
                _mainView.Move(movement * shift);
                _mainView.Zoom(zoom);
                _window.SetView(_mainView);
                _window.Draw(_room);
                _window.Draw(_stage);
                foreach (var musicianConnections in _connections)
                {
                    _window.Draw(musicianConnections.Value);
                }

                foreach (var attendee in _attendees)
                {
                    _window.Draw(attendee);
                }

                foreach (var musician in _musicians)
                {
                    _window.Draw(musician);
                }

                foreach (var pillar in _pillars)
                {
                    _window.Draw(pillar);
                }

                _window.SetView(_hudView);
                if (legend)
                {
                    _window.Draw(helpLabel);
                }

                if (showCoords)
                {
                    _window.Draw(coordsLabel);
                }

                _window.Draw(_currentProblemLabel);
                _window.Display();
            }
        }
    }
}