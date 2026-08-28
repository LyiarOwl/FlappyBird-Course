using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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

    private Rectangle _pixelSrcRect = new Rectangle(585, 449, 16, 16);

    private Vector2 _birdInitialPosition;
    private Bird _bird;

    private Rectangle _flappyBirdLogo = new Rectangle(702, 182, 178, 48);
    private Rectangle _playBtn = new Rectangle(706, 236, 108, 66);
    private Rectangle _gameOverLabel = new Rectangle(786, 118, 200, 52);

    private bool _start;
    private bool _gameOver;
    private bool _scroll;

    private int _score = 0;

    private SpriteFont _gameFont;

    private SoundEffect _dieSfx;
    private SoundEffect _hitSfx;
    private SoundEffect _pointSfx;
    private SoundEffect _wingSfx;

    private SoundEffectInstance _dieSfxInst;
    private SoundEffectInstance _hitSfxInst;
    private SoundEffectInstance _pointSfxInst;
    private SoundEffectInstance _wingSfxInst;

    private bool _collidedWithPipe;

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

        _birdInitialPosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2f,
            _graphics.PreferredBackBufferHeight / 2f
        );
        _bird = new Bird(_texture, _birdInitialPosition);
        _bird.IsCollidingWithFloor += () =>
        {
            _gameOver = true;
            _scroll = false;
            Console.WriteLine("game over");
        };
        _bird.IsCollidingWithPipes += () => { _scroll = false; };
        _bird.Scoring += () => _score++;

        _gameFont = Content.Load<SpriteFont>("GameFont");

        _dieSfx = Content.Load<SoundEffect>("Sounds/sfx_die");
        _hitSfx = Content.Load<SoundEffect>("Sounds/sfx_hit");
        _pointSfx = Content.Load<SoundEffect>("Sounds/sfx_point");
        _wingSfx = Content.Load<SoundEffect>("Sounds/sfx_wing");

        _dieSfxInst = _dieSfx.CreateInstance();
        _hitSfxInst = _hitSfx.CreateInstance();
        _pointSfxInst = _pointSfx.CreateInstance();
        _wingSfxInst = _wingSfx.CreateInstance();

        _bird.Jumping += () =>
        {
            _wingSfxInst.Stop();
            _wingSfxInst.Play();
        };
        _bird.IsCollidingWithFloor += () =>
        {
            if (!_collidedWithPipe)
            {
                _hitSfxInst.Stop();
                _hitSfxInst.Play();
            }
        };
        _bird.IsCollidingWithPipes += () =>
        {
            _dieSfxInst.Stop();
            _dieSfxInst.Play();
            
            _hitSfxInst.Stop();
            _hitSfxInst.Play();
            _collidedWithPipe = true;
        };
        _bird.Scoring += () =>
        {
            _pointSfxInst.Stop();
            _pointSfxInst.Play();
        };
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardExtended.Update();

        if (KeyboardExtended.IsKeyDown(Keys.Escape))
            Exit();

        if (KeyboardExtended.IsKeyJustPressed(Keys.Space) && !_start)
        {
            _start = true;
            _scroll = true;
        }

        if (_start)
        {
            if (_scroll)
            {
                ScrollBackground();
                ScrollGround();

                HandlePipesSpawning(gameTime);
                ScrollPipes();
                DestroyPipesOutsideScreen();
            }

            if (!_gameOver)
                _bird.Update(_pipes);

            if (_gameOver)
            {
                if (KeyboardExtended.IsKeyJustPressed(Keys.Space))
                    Reset();
            }
        }


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
            pipe.Draw(_spriteBatch, _pixelSrcRect);

        _spriteBatch.Draw(_texture, _groundPos1, _groundSrcRect, Color.White);
        _spriteBatch.Draw(_texture, _groundPos2, _groundSrcRect, Color.White);

        _bird.Draw(_spriteBatch, _pixelSrcRect);

        Vector2 center = new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight) / 2f;
        Vector2 size = _gameFont.MeasureString(_score.ToString());
        Vector2 origin = new Vector2(size.X / 2f, 0f);

        _spriteBatch.DrawString(_gameFont, _score.ToString(), new Vector2(center.X, 12f), Color.Black * 0.5f, 0f,
            origin, 1f, SpriteEffects.None, 0f);

        _spriteBatch.DrawString(_gameFont, _score.ToString(), new Vector2(center.X, 10f), Color.White, 0f, origin, 1f,
            SpriteEffects.None, 0f);

        if (!_start)
        {
            _spriteBatch.Draw(_texture,
                center + new Vector2(-(_flappyBirdLogo.Width / 2f), -150f),
                _flappyBirdLogo,
                Color.White);

            _spriteBatch.Draw(_texture,
                center + new Vector2(-(_playBtn.Width / 2f), 100f),
                _playBtn,
                Color.White);
        }

        if (_gameOver)
        {
            _spriteBatch.Draw(_texture,
                center - _gameOverLabel.Size.ToVector2() / 2,
                _gameOverLabel,
                Color.White);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void Reset()
    {
        _bird.Reset();
        _bird.Position = _birdInitialPosition;

        _pipes.Clear();
        _scroll = false;
        _start = false;
        _gameOver = false;
        _pipesSpawnElapsed = 0f;
        _score = 0;
        _collidedWithPipe = false;
    }

    protected override void Dispose(bool disposing)
    {
        _texture.Dispose();
        _spriteBatch.Dispose();
        _gameFont.Texture.Dispose();
        _dieSfx.Dispose();
        _hitSfx.Dispose();
        _pointSfx.Dispose();
        _wingSfx.Dispose();
        base.Dispose(disposing);
    }
}