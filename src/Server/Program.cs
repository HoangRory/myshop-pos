using LuciferCore.Main;
using Server.Core;

Lucifer.CMD("/init di");
Lucifer.CMD("/check license"u8);

if (!AppLicense.IsValid)
{
    System.Environment.ExitCode = 1;
    return;
}
Lucifer.CMD("/run"u8);

