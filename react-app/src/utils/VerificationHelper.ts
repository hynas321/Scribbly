class VerificationHelper
{
  getGameHash(url: string)
  {
    const lastSlashIndex = url.lastIndexOf('/');
    return url.substring(lastSlashIndex + 1);
  }
}

export default VerificationHelper;