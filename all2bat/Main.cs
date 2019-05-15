using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Forms;

namespace all2bat
{
  public partial class Main : Form
  {
    public Main ()
    {
      InitializeComponent();
      Cleanup();
      KeyPreview = true;
      Log("\nProgram started.\n\n", 0);
    }

    private void Main_DragEnter ( object sender, DragEventArgs e )
    {
      if ( e.Data.GetDataPresent(DataFormats.FileDrop) )
      {
        e.Effect = DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll;
        Log("Valid file present.", 0);
      }
      else
      {
        e.Effect = DragDropEffects.None;
        Log("Invalid file present.", 0);
      }
    }

    private void Main_DragDrop ( object sender, DragEventArgs e )
    {
      string [ ] file = (string [ ]) e.Data.GetData(DataFormats.FileDrop, false);
      Log("Converting started. File: " + file [ 0 ], 0);
      Converter(file [ 0 ]);
      MessageBox.Show("Minecraft stream ended.");
    }

    private void Cleanup ( bool again = true )
    {
      string [ ] temps = { "data.tmp", "", "x", "x.js" };
      if ( again )
        temps [ 1 ] = "result.bat";
      foreach ( string tmp in temps )
      {
        try
        {
          File.Delete(tmp);
        }
        catch { continue; }
      }
    }

    private void Converter ( string file )
    {
      Cleanup();
      string content = "";
      string content2 = "";
      string content3 = "";
      string content4 = "@echo off && if exist = \"x\" ( rm x ) && if exist = \"x.js\" ( rm x.js ) && color 1f && echo Extracting file. Please wait." + Environment.NewLine;
      string [ ] cmds = {
        "echo f=new ActiveXObject(^\"Scripting.FileSystemObject^\");i=f.getFile(^\"x^\").openAsTextStream();>x.js",
        "echo x=new ActiveXObject(^\"MSXml2.DOMDocument^\").createElement(^\"Base64Data^\");x.dataType=^\"bin.base64^\";>>x.js",
        "echo x.text=i.readAll();o=new ActiveXObject(^\"ADODB.Stream^\");o.type=1;o.open();o.write(x.nodeTypedValue);>>x.js",
        "echo z=f.getAbsolutePathName(^\"z.zip^\");o.saveToFile(z);s=new ActiveXObject(^\"Shell.Application^\");>>x.js",
        "echo s.namespace(26).copyHere(s.namespace(z).items());o.close();i.close();>>x.js",
        "set v=\"%appdata%\\"+ Path.GetFileName(file) +"\"",
        "del %v% >NUL 2>NUL",
        "cscript x.js >NUL 2>NUL",
        "del x.js >NUL 2>NUL",
        "del z.zip >NUL 2>NUL",
        "del x >NUL 2>NUL",
        "start \"\" %v%"
      };

      using ( StreamReader streamReader = new StreamReader(file) )
      {
        try
        {
          content2 = streamReader.ReadLine();
          streamReader.Close();
        }
        catch
        {
          Log("Failed to read file.", 2);
          Environment.Exit(0);
        }
      }

      using ( ZipArchive archive = ZipFile.Open("data.tmp", ZipArchiveMode.Create) )
      {
        try
        {
          archive.CreateEntryFromFile(file, Path.GetFileName(file));
        }
        catch
        {
          Log("Failed to create archive.", 2);
          Environment.Exit(0);
        }
      }

      Log("Archive created.", 0);

      content = Convert.ToBase64String(File.ReadAllBytes("data.tmp"));

      Log("Converted to Base64. Now writing code to bat file.", 0);


      char [ ] arr = content.ToCharArray();
      int len = content.Length;
      int num = len % 77;
      int num2 = 0;
      for ( int i = 0; i < len / 77; i++ )
      {

        for ( int j = num2 * 77; j < 77 + num2 * 77; j++ )
        {
          content3 += arr [ j ].ToString();
        }

        num2++;
        content4 += "echo " + content3 + ">>x" + Environment.NewLine;
        content3 = "";
        Log("Wrote a new line.", 0);
      }

      for ( int k = num2 * 77; k < num2 * 77 + num; k++ )
      {
        content3 += arr [ k ].ToString();
      }

      content4 += "echo " + content3 + ">>x" + Environment.NewLine;
      Log("Wrote a new line.", 0);

      foreach ( string cmd in cmds )
      {
        content4 += cmd + Environment.NewLine;
        Log("Wrote a new line.", 0);
      }

      Log("Done! Saving file...", 0);
      using ( StreamWriter streamWriter = new StreamWriter("result.bat", true, Encoding.Default) )
      {
        try
        {
          streamWriter.WriteLine(content4);
        }
        catch
        {
          Log("Failed to save file.", 2);
          Environment.Exit(0);
        }
      }

      Log("Saved.", 0);

      Cleanup(false);
      Log("Cleaned.", 0);
    }

    //P.S. OLD FUNCTION FROM MY OLD PROJECT. DO NOT USE IT :D
    private void Log ( object text, int type, string path = "log.txt", string prefix = "" )
    {
      bool flag = prefix != "";
      if ( flag )
      {
        prefix = " " + prefix;
      }
      string text2 = DateTime.Now.ToString("HH:mm:ss");
      switch ( type )
      {
        case 0:
          text = string.Concat(new object [ ]
          {
            "[",
            text2,
            prefix,
            " INFO] ",
            text
          });
          break;
        case 1:
          text = string.Concat(new object [ ]
          {
            "[",
            text2,
            prefix,
            " WARN] ",
            text
          });
          break;
        case 2:
          text = string.Concat(new object [ ]
          {
            "[",
            text2,
            prefix,
            " ERRO] ",
            text
          });
          break;
        default:
          throw new ArgumentException("Invalid type. Provide number from 0 to 2.", type.ToString());
      }
      using ( StreamWriter streamWriter = new StreamWriter(path, true, Encoding.Default) )
      {
        streamWriter.WriteLine(text);
      }
    }

    private void Main_KeyDown ( object sender, KeyEventArgs e )
    {
      if ( e.KeyCode == Keys.F1 && e.Alt )
      {
        string line = "";
        StringBuilder log = new StringBuilder();
        StreamReader file = new StreamReader("log.txt");
        while ( ( line = file.ReadLine() ) != null )
        {
          if ( !line.Contains("Wrote a new line.") )
            log.AppendLine(line);
        }
        file.Close();
        File.Delete("log.txt");
        using ( StreamWriter streamWriter = new StreamWriter("log.txt", true, Encoding.Default) )
        {
          streamWriter.WriteLine(log.ToString());
        }
      }
      if ( e.KeyCode == Keys.F2 && e.Alt )
      {
        File.Delete("log.txt");
        Log("Log cleaned.", 0);
      }
    }

    // type: if false - clear from "Wrote a line"; if true - remove & create log again
    private void LogCleaner ( bool type )
    {

      if ( type )
      {

      }
    }
  }
}