# RemoteKeys – or, how to shoot yourself in the foot, probably

Before anything else: if you don't immediately see how running an HTTP listener
that can trigger keyboard presses can be extremely problematic, do not use this
tool. Also, if you don't know how to read the code in this little project and
decide whether it is safe for you to use, do not use this tool.

If you do decide to use this tool, make a backup of your machine first, because
who knows what kind of unfunny mistakes you will make when you try it out.


## Why does this thing exist?

I'm using a couple of Elgato Stream Deck units to make my life easier. One big
reason for my purchase of a Stream Deck was to map a number of shortcuts in
Visual Studio, which I use over RDP from my Mac. Turns out you can't send
keyboard shortcuts to Microsoft Remote Desktop and expect them to show up on
the remote computer. Well, now you can.


## Usage

Clone this repository. Change appsettings.json to your liking. Make sure the
interface and port to which you bind the HTTP listener is not accessible from
the internet, unless you really want to invite trouble. Build and run the thing.

The syntax is simple:

    /key # sends a key
    /key+otherkey # sends key combined with otherkey, for example ctrl+a
    /key/key # sends key, then key
    /delay/n # causes a delay of n milliseconds
    /text/sometext # types sometext for you

These can be combined into a complex path, like so:

    /ctrl+a/ctrl+c/delay/200/win+r/delay/100/text/notepad/enter/delay/200/ctrl+v

Obviously, the more you push this thing, the greater the likelyhood that things
will go spectacularly wrong.


### Stream Deck

I use this with Stream Deck, but you can obviously use it however you like. For
Stream Deck, find a plugin that enables you to make HTTP requests and call the
machine running this tool with various paths, triggering whatever shortcuts
you like.


### F13-F24

Your keyboard probably does not have keys for F13 through F24, but they are
available to use. Send with this tool to map in your destination application if
the configuration of that application is not manually editable (like VS).


## Troubleshooting

If you get Access denied, start in an elevated shell. Otherwise, read the
output and figure it out.