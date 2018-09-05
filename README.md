# DDS Reader

A .NET Core DDS (Direct Draw Surface) reader and converter. It is composed of a development library and a console application.

The library is a simple wrapper around two great packages:

- [Pfim](https://github.com/nickbabcock/Pfim) DDS and TGA decoder
- [ImageSharp](https://github.com/SixLabors/ImageSharp) cross-platform 2D graphics API

The console application uses the library as a quick way to convert a single DDS file to some popular image formats (jpg, png, gif, bmp).

These features meet the requirements for this projects goal. Any additional features are unlikely to be added. As this project is a simple wrapper, I encourage you to use the great libraries mentioned above directly in your own projects.

## Usage

The library contains a single `DDSImage` class, which can instantiate objects from *bytes*, *streams* or *files*.

```
DDSImage img = new DDSImage("/path/to/dds");
```

Access the raw data with `img.Data` or save the image in another format with `img.Save("img.png")`.

## Previous Version

A previous version of this project used a C# DDS decoder cobbled together from a couple of sources. It was unreliable and difficult to maintain. For these reasons it was scrapped in favour of using the modern libraries referenced above.

## License

DDSReader is available under the [MIT license](./LICENSE). Project dependencies may have different licensing terms, consult relevant projects for more details.