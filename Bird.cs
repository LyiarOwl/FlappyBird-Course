using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FlappyBirdCourse;

public class Bird
{
    private readonly Texture2D _texture;
    private Vector2 _position;

    private Rectangle _srcRect = new Rectangle(6, 982, 34, 24);

    public Rectangle Collider = new Rectangle(0, 0, 18, 18);

    public Vector2 Position
    {
        get => _position;
        set => _position = value;
    }

    public Vector2 Velocity;
    public float Gravity = 0.25f;
    public float JumpImpulse = 4f;

    public event Action IsCollidingWithFloor;
    public event Action IsCollidingWithPipes;
    public event Action Scoring;
    public event Action Jumping;
    private bool _isCollidingWithPipe;
    private bool _enteredScoringCollider;

    public Bird(Texture2D texture, Vector2 position)
    {
        _texture = texture;
        Position = position;
    }

    public void Update(List<Pipe> pipes)
    {
        Velocity.Y += Gravity;

        HandleGroundCollision();
        HandlePipesCollisions(pipes);

        if (!_isCollidingWithPipe)
        {
            if (KeyboardExtended.IsKeyJustPressed(Keys.Space))
            {
                Velocity.Y = -JumpImpulse;
                Jumping?.Invoke();
            }

            bool isInsideCollider = false;
            foreach (var pipe in pipes)
            {
                if (Collider.Intersects(pipe.ScoringCollider))
                {
                    isInsideCollider = true;
                    break;
                }
            }

            if (isInsideCollider && !_enteredScoringCollider)
            {
                _enteredScoringCollider = true;
                Scoring?.Invoke();
            }
            else if (!isInsideCollider)
            {
                _enteredScoringCollider = false;
            }
        }

        Position += Velocity;

        Collider.Location = Position.ToPoint() - Collider.Size / 2;
    }

    private void HandleGroundCollision()
    {
        float groundY = 400f;
        if (Position.Y + Velocity.Y >= groundY)
        {
            Velocity.Y = 0f;
            Position = new Vector2(Position.X, groundY);
            IsCollidingWithFloor?.Invoke();
        }
    }

    private void HandlePipesCollisions(List<Pipe> pipes)
    {
        foreach (var pipe in pipes)
        {
            if (Collider.Intersects(pipe.BottomPipeCollider) || Collider.Intersects(pipe.TopPipeCollider))
            {
                if (!_isCollidingWithPipe)
                    IsCollidingWithPipes?.Invoke();
                _isCollidingWithPipe = true;
            }
        }
    }

    public void Draw(SpriteBatch batch, Rectangle pixelSrcRect)
    {
        Vector2 origin = _srcRect.Size.ToVector2() / 2f;
        batch.Draw(_texture, Position, _srcRect, Color.White, 0f,
            origin, 1f, SpriteEffects.None, 0f);

        // batch.Draw(_texture, Collider, pixelSrcRect, Color.HotPink * 0.5f);
    }

    public void Reset()
    {
        Velocity = Vector2.Zero;
        _isCollidingWithPipe = false;
        _enteredScoringCollider = false;
    }
}