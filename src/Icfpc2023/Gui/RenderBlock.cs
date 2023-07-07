using SFML.Graphics;

namespace Icfpc2023.Gui;

/*public class RenderBlock: Transformable, Drawable
{
    public Text id;
    public VertexArray lines = new VertexArray(PrimitiveType.LineStrip);
    public void Draw(RenderTarget target, RenderStates states)
    {
        states.Transform *= Transform;
        states.Transform.Scale(Render.RenderScale, Render.RenderScale);
        target.Draw(lines, states);
        if (id != null)
        {
            target.Draw(id, states);
        }
    }
}*/