using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlappyBirdCourse;

public class Pipe
{
    private readonly Texture2D _texture;
    private Vector2 _position;
    private Rectangle _topPipeSrcRect = new Rectangle(112, 646, 52, 320);
    private Rectangle _bottomPipeSrcRect = new Rectangle(168, 646, 52, 320);

    public Vector2 TopPipePosition { get; private set; }
    public Vector2 BottomPipePosition { get; private set; }
    public Rectangle TopPipeCollider = new Rectangle(0, 0, 52, 320);
    public Rectangle BottomPipeCollider = new Rectangle(0, 0, 52, 320);
    public Rectangle ScoringCollider = new Rectangle(0, 0, 10, 80);

    public Vector2 Position
    {
        get => _position;
        set
        {
            float halfPipeWidth = _topPipeSrcRect.Width / 2f;
            float pipeHeight = _topPipeSrcRect.Height;
            float halfGap = GapBetweenPipes / 2f;

            TopPipePosition = new Vector2(value.X - halfPipeWidth, value.Y - pipeHeight - halfGap);
            BottomPipePosition = new Vector2(value.X - halfPipeWidth, value.Y + halfGap);

            TopPipeCollider.Location = TopPipePosition.ToPoint();
            BottomPipeCollider.Location = BottomPipePosition.ToPoint();
            ScoringCollider.Location = value.ToPoint() - ScoringCollider.Size / 2;
            
            _position = value;
        }
    }

    public float GapBetweenPipes = 90f;

    public Pipe(Texture2D texture, Vector2 position)
    {
        _texture = texture;
        Position = position;
    }

    public void Draw(SpriteBatch batch, Rectangle pixelSrcRect)
    {
        batch.Draw(_texture, TopPipePosition, _topPipeSrcRect, Color.White);
        batch.Draw(_texture, BottomPipePosition, _bottomPipeSrcRect, Color.White);

        // batch.Draw(_texture, TopPipeCollider, pixelSrcRect, Color.Red * 0.5f);
        // batch.Draw(_texture, BottomPipeCollider, pixelSrcRect, Color.Red * 0.5f);
        // batch.Draw(_texture, ScoringCollider, pixelSrcRect, Color.Blue * 0.5f);
    }
}