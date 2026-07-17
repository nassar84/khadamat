namespace Khadamat.WebAPI.Services;

public class ShareCardGenerator
{
    private readonly string _wwwrootPath;

    public ShareCardGenerator(string wwwrootPath, string? fontsPath = null)
    {
        _wwwrootPath = wwwrootPath;
    }
}
