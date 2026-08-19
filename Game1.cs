using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FlappyBirdCourse;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _texture;

    private float _bgScrollSpeed = 0.5f;
    private float _groundScrollSpeed = 2f;

    private Rectangle _bgSrcRect = new Rectangle(0, 0, 288, 512);

    private Vector2 _bgPos1;
    private Vector2 _bgPos2 = new Vector2(288f, 0f);

    private Rectangle _groundSrcRect = new Rectangle(584, 0, 336, 112);
    private Vector2 _groundPos1;
    private Vector2 _groundPos2 = new Vector2(336f, 0f);

    private List<Pipe> _pipes = [];
    private Vector2 _initialPipesPosition;

    private float _pipesSpawnInterval = 1.5f;
    private float _pipesSpawnElapsed;
    private float _maxDelta = 1f / 20f;

    private float _minPipeY = 130f;
    private float _maxPipeY = 290f;
    private MathHelper.Random _rng = new MathHelper.Random();

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

        var windowHeight = _graphics.PreferredBackBufferHeight;
        _groundPos1.Y = windowHeight - _groundSrcRect.Height;
        _groundPos2.Y = _groundPos1.Y;

        _initialPipesPosition = new Vector2(
            _graphics.PreferredBackBufferWidth + 30f, // window width + 30px 
            _graphics.PreferredBackBufferHeight / 2f
        );

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

        ScrollBackground();
        ScrollGround();

        HandlePipesSpawning(gameTime);
        ScrollPipes();
        DestroyPipesOutsideScreen();

        base.Update(gameTime);
    }

    private void ScrollBackground()
    {
        _bgPos1.X -= _bgScrollSpeed;
        if (_bgPos1.X + _bgSrcRect.Width < 0f)
            _bgPos1.X = 0f;

        _bgPos2.X -= _bgScrollSpeed;
        var windowWidth = _graphics.PreferredBackBufferWidth;
        if (_bgPos2.X + _bgSrcRect.Width < windowWidth)
            _bgPos2.X = windowWidth;
    }

    private void ScrollGround()
    {
        _groundPos1.X -= _groundScrollSpeed;
        if (_groundPos1.X + _groundSrcRect.Width < 0f)
            _groundPos1.X = 0f;

        _groundPos2.X -= _groundScrollSpeed;
        var windowWidth = _graphics.PreferredBackBufferWidth;
        if (_groundPos2.X + _groundSrcRect.Width < windowWidth)
            _groundPos2.X = windowWidth;
    }

    private void HandlePipesSpawning(GameTime gameTime)
    {
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float frameTime = MathF.Min(_maxDelta, delta);
        _pipesSpawnElapsed += frameTime;
        if (_pipesSpawnElapsed >= _pipesSpawnInterval)
        {
            CreatePipe();
            _pipesSpawnElapsed -= _pipesSpawnInterval;
        }
    }

    private void CreatePipe()
    {
        var position = new Vector2(
            _initialPipesPosition.X,
            _rng.NextFloat(_minPipeY, _maxPipeY)
        );
        var pipe = new Pipe(_texture, position);
        _pipes.Add(pipe);
    }

    private void ScrollPipes()
    {
        foreach (var pipe in _pipes)
        {
            var position = pipe.Position;
            position.X -= _groundScrollSpeed;
            pipe.Position = position;
        }
    }

    private void DestroyPipesOutsideScreen()
    {
        for (int i = 0; i < _pipes.Count; i++)
        {
            var pipe = _pipes[i];
            if (pipe.Position.X + 30f < 0f)
                _pipes.RemoveAt(i);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_texture, _bgPos1, _bgSrcRect, Color.White);
        _spriteBatch.Draw(_texture, _bgPos2, _bgSrcRect, Color.White);

        foreach (var pipe in _pipes)
            pipe.Draw(_spriteBatch);

        _spriteBatch.Draw(_texture, _groundPos1, _groundSrcRect, Color.White);
        _spriteBatch.Draw(_texture, _groundPos2, _groundSrcRect, Color.White);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        _texture.Dispose();
        _spriteBatch.Dispose();
        base.Dispose(disposing);
    }
}