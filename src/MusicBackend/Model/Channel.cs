using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Model;

public struct Channel
{
    public float left,
        right;

    public static Channel operator +(Channel a, Channel b) =>
        new() { left = a.left + b.left, right = a.right + b.right };

    public static Channel operator *(Channel a, float b) =>
        new() { left = a.left * b, right = a.right * b };

    public static Channel operator *(float a, Channel b) =>
        new() { left = a * b.left, right = a * b.right };

    public static Channel operator -(Channel a, Channel b) =>
        new() { left = a.left - b.left, right = a.right - b.right };

    public static Channel operator /(Channel a, float b) =>
        new() { left = a.left / b, right = a.right / b };
}
