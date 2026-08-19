using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FlappyBirdCourse;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _texture;
    
    private float _bgScrollSpeed = 1f;

    private Rectangle _bgSrcRect = new Rectangle(0, 0, 288, 512);

    private Vector2 _bgPos1;
    private Vector2 _bgPos2 = new Vector2(288f, 0f);

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = 288;
        _graphics.PreferredBackBufferHeight = 512;
        _graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _texture = Content.Load<Texture2D>("Graphics/spritesheet");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

         _bgPos1.X -= _bgScrollSpeed;
         if (_bgPos1.X + _bgSrcRect.Width < 0f)
             _bgPos1.X = 0f;

         var windowWidth = _graphics.PreferredBackBufferWidth;
         _bgPos2.X -= _bgScrollSpeed;
         if (_bgPos2.X + _bgSrcRect.Width < windowWidth)
             _bgPos2.X = windowWidth;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_texture, _bgPos1, _bgSrcRect, Color.White);
        _spriteBatch.Draw(_texture, _bgPos2, _bgSrcRect, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        _texture.Dispose();
        base.Dispose(disposing);
    }
}