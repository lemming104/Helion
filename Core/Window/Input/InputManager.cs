﻿using System;
using System.Collections.Generic;
using System.Linq;
using Helion.Geometry.Vectors;
using Helion.Util.Container;

namespace Helion.Window.Input
{
    /// <summary>
    /// A simple implementation of an input manager.
    /// </summary>
    public class InputManager : IInputManager
    {
        public Vec2I MouseMove { get; private set; } = (0, 0);
        // public bool PrintScreenPressed { get; private set; }
        private readonly ConsumableInput m_consumableInput;
        private readonly HashSet<Key> m_inputDown = new();
        private readonly HashSet<Key> m_inputPrevDown = new();
        private readonly DynamicArray<char> m_typedCharacters = new();
        private double m_mouseScroll;

        public int Scroll => (int)m_mouseScroll;
        public ReadOnlySpan<char> TypedCharacters => new(m_typedCharacters.Data, 0, m_typedCharacters.Length);

        public InputManager()
        {
            m_consumableInput = new ConsumableInput(this);
        }

        public void SetKeyDown(Key key)
        {
            // For some stupid reason, the print screen key does something very
            // weird under the hood that locks everything up, causing the polling
            // loop from OpenTK to emit Down/Up right after each other, which
            // clears it. This forces my hand to write this hacky workaround until
            // I ask the devs what is going on.
            // if (key == Key.PrintScreen)
            //     PrintScreenPressed = true;
            
            m_inputDown.Add(key);
        }
        
        public void SetKeyUp(Key key)
        {
            if (key == Key.PrintScreen)
                return;
            
            m_inputDown.Remove(key);
        }

        public void AddTypedCharacters(ReadOnlySpan<char> str)
        {
            for (int i = 0; i < str.Length; i++)
                m_typedCharacters.Add(str[i]);
        }

        public void AddMouseMovement(Vec2I movement)
        {
            MouseMove += movement;
        }
        
        public void AddMouseScroll(double amount)
        {
            m_mouseScroll += amount;
        }

        public bool IsKeyDown(Key key) => m_inputDown.Contains(key);
        public bool IsKeyPrevDown(Key key) => m_inputPrevDown.Contains(key);
        public bool IsKeyHeldDown(Key key) => IsKeyDown(key) && IsKeyPrevDown(key);
        public bool IsKeyUp(Key key) => !m_inputDown.Contains(key);
        public bool IsKeyPrevUp(Key key) => !m_inputPrevDown.Contains(key);
        public bool IsKeyPressed(Key key) => IsKeyDown(key) && !IsKeyPrevDown(key);
        public bool IsKeyReleased(Key key) => !IsKeyDown(key) && IsKeyPrevDown(key);
        public bool HasAnyKeyPressed() => m_inputDown.Any(IsKeyPressed);
        public bool HasAnyKeyDown() => m_inputDown.Any();
        // public bool IsKeyDown(Key key) => key == Key.PrintScreen ? PrintScreenPressed : m_inputDown.Contains(key);
        // public bool IsKeyPrevDown(Key key) => m_inputPrevDown.Contains(key);
        // public bool IsKeyHeldDown(Key key) => IsKeyDown(key) && IsKeyPrevDown(key);
        // public bool IsKeyUp(Key key) => key == Key.PrintScreen ? !PrintScreenPressed : !m_inputDown.Contains(key);
        // public bool IsKeyPrevUp(Key key) => !m_inputPrevDown.Contains(key);
        // public bool IsKeyPressed(Key key) => key == Key.PrintScreen ? PrintScreenPressed : IsKeyDown(key) && !IsKeyPrevDown(key);
        // public bool IsKeyReleased(Key key) => !IsKeyDown(key) && IsKeyPrevDown(key);
        // public bool HasAnyKeyPressed() => PrintScreenPressed || m_inputDown.Any(IsKeyPressed);
        // public bool HasAnyKeyDown() => PrintScreenPressed || m_inputDown.Any();

        public void Reset()
        {
            MouseMove = (0, 0);
            m_mouseScroll = 0;
            m_typedCharacters.Clear();
            m_inputPrevDown.Clear();
            foreach (Key key in m_inputDown)
                m_inputPrevDown.Add(key);

            m_inputDown.Remove(Key.PrintScreen);
            // if (PrintScreenPressed)
            //     m_inputPrevDown.Add(Key.PrintScreen);
            // PrintScreenPressed = false;
        }
        
        public IConsumableInput Poll()
        {
            m_consumableInput.Reset();
            return m_consumableInput;
        }
    }
}
