using PlayerApi.Interfaces;
using PlayerApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerApi.Filters
{
    public sealed class SmoothingFilter : RecursiveFilter
    {
        private float dampingFactor;
        public SmoothingFilter(float dampingFactor) { this.dampingFactor = dampingFactor; }
        public override Channel processChannels(Channel channels, int index)
        {
            var prevChannel = previousBuffer[index];
            channels.left -= prevChannel.left;
            channels.right -= prevChannel.right;
            previousBuffer[index] = new()
            {
                left = prevChannel.left * dampingFactor,
                right = prevChannel.right * dampingFactor
            };
            return channels;
        }

    }
}
