ColorSuite
==========

![Screenshot 1](http://keijiro.github.io/ColorSuite/screenshot1.png)

![Screenshot 2](http://keijiro.github.io/ColorSuite/screenshot2.png)

ColorSuite is an color adjustment image effect for Unity, which provides multiple
functions in a single component.

- Tonemapping (simplified Reinhard)
- Curves (red, green, blue and luminance)
- Brightness, contrast and saturation control
- Vignetting

These functions are implemented in a single shader, and unused functions are
automatically removed from the shader using the multiple shader compilation
technique. It guarantees the best performance in any case.

License
-------

Copyright (C) 2014 Keijiro Takahashi

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
