using MusicBackend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Interfaces;

public interface IFilter
{
    /**
     * Process the buffer that is about to be displayed 
     * returns new buffer, can be same as parameter
     */
    Channel[] process(Channel[] buffer);

    /**
     * Called when new buffer from sound device is obtained
     */
    void refresh(Channel[] newBuffer) {}
}
