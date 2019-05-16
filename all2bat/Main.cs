//Main.cs created by AlexeyZavar
//Licensed under GPL v3

namespace all2bat
{
  using System;
  using System.IO;
  using System.IO.Compression;
  using System.Text;
  using System.Threading.Tasks;
  using System.Windows.Forms;

  /// <summary>
  /// Defines the <see cref="Main" />
  /// </summary>
  public partial class Main : Form
  {
    /// <summary>
    /// Defines the debug option
    /// </summary>
    private bool debug = false;

    /// <summary>
    /// Defines the filename
    /// </summary>
    private string name = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="Main"/> class.
    /// </summary>
    public Main ()
    {
      InitializeComponent();
      Cleanup();
      KeyPreview = true;
      Log("Program started.\n\n", 0);
    }

    /// <summary>
    /// Drag Enter
    /// </summary>
    /// <param name="sender">Sender<see cref="object"/></param>
    /// <param name="e">Args<see cref="DragEventArgs"/></param>
    private void Main_DragEnter ( object sender, DragEventArgs e )
    {
      if ( e.Data.GetDataPresent(DataFormats.FileDrop) )
      {
        string [ ] file = (string [ ]) e.Data.GetData(DataFormats.FileDrop, false);
        if ( File.Exists(( file [ 0 ] )) ) //It can be folder, so let's check it
        {
          e.Effect = DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll; //Effects
          Log("Valid file present.", 0, true); //Ok
        }
        else
        {
          e.Effect = DragDropEffects.None; //Effects
          Log("Invalid file present.", 0, true); //Not ok
        }
      }
      else
      {
        e.Effect = DragDropEffects.None; //Effects
        Log("Invalid file present.", 0, true); //Not ok
      }
    }

    /// <summary>
    /// Drag Drop
    /// </summary>
    /// <param name="sender">Sender<see cref="object"/></param>
    /// <param name="e">Args<see cref="DragEventArgs"/></param>
    private void Main_DragDrop ( object sender, DragEventArgs e )
    {
      string [ ] file = (string [ ]) e.Data.GetData(DataFormats.FileDrop, false);
      Log("Converting started. File: " + file [ 0 ], 0);
      TopMost = false;
      Task.Run(() => Converter(file [ 0 ])).ContinueWith(end => Done());
    }

    /// <summary>
    /// MessageBox function
    /// </summary>
    private void Done ()
    {
      MessageBox.Show("File saved: " + Environment.CurrentDirectory + "\\" + name + ".bat");
    }

    /// <summary>
    /// Cleanup program folder
    /// </summary>
    /// <param name="again">The again<see cref="bool"/></param>
    private void Cleanup ( bool again = true )
    {
      string [ ] temps = { "data.tmp", "", "x", "x.js" };
      if ( again )
      {
        temps [ 1 ] = name;
        name = "";
      }
      foreach ( string tmp in temps )
      {
        try
        {
          File.Delete(tmp);
        }
        catch { continue; }
      }
    }

    /// <summary>
    /// Converter
    /// </summary>
    /// <param name="file">File path<see cref="string"/></param>
    private void Converter ( string file )
    {
      Cleanup();
      name = Path.GetFileName(file);
      //Empty variables for work
      string content = "";
      string content2 = "";
      //result.bat content
      string content3 = "@echo off \n if exist = \"x\" ( rm x ) \n if exist = \"x.js\" ( rm x.js ) \n color 1f \n echo Extracting file. Please wait.";
      //Commands for x.js file
      string [ ] cmds = {
        "echo f=new ActiveXObject(^\"Scripting.FileSystemObject^\");i=f.getFile(^\"x^\").openAsTextStream();>x.js",
        "echo x=new ActiveXObject(^\"MSXml2.DOMDocument^\").createElement(^\"Base64Data^\");x.dataType=^\"bin.base64^\";>>x.js",
        "echo x.text=i.readAll();o=new ActiveXObject(^\"ADODB.Stream^\");o.type=1;o.open();o.write(x.nodeTypedValue);>>x.js",
        "echo z=f.getAbsolutePathName(^\"z.zip^\");o.saveToFile(z);s=new ActiveXObject(^\"Shell.Application^\");>>x.js",
        "echo s.namespace(26).copyHere(s.namespace(z).items());o.close();i.close();>>x.js",
        "set v=\"%appdata%\\"+ name +"\"",
        "del %v% >NUL 2>NUL",
        "cscript x.js >NUL 2>NUL",
        "del x.js >NUL 2>NUL",
        "del z.zip >NUL 2>NUL",
        "del x >NUL 2>NUL",
        "start \"\" %v%",
        "echo File saved: %v%",
        "pause >NUL"
      };

      //Read file contents in $content2
      using ( StreamReader streamReader = new StreamReader(file) )
      {
        try
        {
          content2 = streamReader.ReadLine();
        }
        catch
        {
          Error("Failed to read file.");
        }
        streamReader.Close();
      }

      //Create ZipArchive
      using ( ZipArchive archive = ZipFile.Open("data.tmp", ZipArchiveMode.Create) )
      {
        try
        {
          archive.CreateEntryFromFile(file, Path.GetFileName(file));
        }
        catch
        {
          Error("Failed to create archive.");
        }
      }

      Log("Archive created.", 0);

      //Convert archive to Base64
      content = Convert.ToBase64String(File.ReadAllBytes("data.tmp"));

      Log("Converted to Base64. Now writing code to bat file.", 0);

      content3 += "\necho " + content + ">>x\n";
      Log("Wrote a new line.", 0, true);

      //Write x.js commands
      foreach ( string cmd in cmds )
      {
        content3 += cmd + Environment.NewLine;
        Log("Wrote a new line.", 0, true);
      }

      Log("Done! Saving file...", 0);

      //Save file
      using ( StreamWriter streamWriter = new StreamWriter(name + ".bat", true, Encoding.Default) )
      {
        try
        {
          streamWriter.WriteLine(content3);
        }
        catch
        {
          Error("Failed to save file.");
        }
      }

      Log("Saved.", 0);

      Cleanup(false);
      Log("Cleaned.", 0);
    }

    /// <summary>
    /// Logger
    /// </summary>
    /// <param name="text">Text<see cref="object"/></param>
    /// <param name="type">Type<see cref="int"/></param>
    /// <param name="path">Path to log file<see cref="string"/></param>
    /// <param name="prefix">Prefix<see cref="string"/></param>
    private void Logger ( object text, int type, string path = "log.txt", string prefix = "" )
    {
      bool flag = prefix != "";
      if ( flag )
        prefix = " " + prefix; //Set prefix if present
      string time = DateTime.Now.ToString("HH:mm:ss"); //Current time
      switch ( type ) //String generator
      {
        case 0: //Information
          text = string.Concat(new object [ ]
          {
            "[",
            time,
            prefix,
            " INFO] ",
            text
          });
          break;
        case 1: //Warning
          text = string.Concat(new object [ ]
          {
            "[",
            time,
            prefix,
            " WARN] ",
            text
          });
          break;
        case 2: //Error
          text = string.Concat(new object [ ]
          {
            "[",
            time,
            prefix,
            " ERRO] ",
            text
          });
          break;
        default: //Invalid type
          throw new ArgumentException("Invalid type. Provide number from 0 to 2.", type.ToString());
      }
      using ( StreamWriter streamWriter = new StreamWriter(path, true, Encoding.Default) )
      {
        streamWriter.WriteLine(text); //Log2File writer
      }
    }

    /// <summary>
    /// HotKeys
    /// </summary>
    /// <param name="sender">Sender<see cref="object"/></param>
    /// <param name="e">Args<see cref="KeyEventArgs"/></param>
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
      if ( e.KeyCode == Keys.F3 && e.Alt )
      {
        if ( !debug )
        {
          debug = true;
          Log("Debug mode activated.", 1);
        }
        else
        {
          debug = false;
          Log("Debug mode deactivated.", 1);
        }
      }
    }

    /// <summary>
    /// Error
    /// </summary>
    /// <param name="msg">Message<see cref="string"/></param>
    private void Error ( string msg )
    {
      Log(msg, 2);
      MessageBox.Show(msg);
      Exit();
    }

    /// <summary>
    /// Exit
    /// </summary>
    private void Exit ()
    {
      Log("Program closed.\n\n", 1);
      Environment.Exit(0);
    }

    /// <summary>
    /// Log
    /// </summary>
    /// <param name="text">Text<see cref="object"/></param>
    /// <param name="type">Type<see cref="int"/></param>
    /// <param name="toDebug">Is a debug message<see cref="bool"/></param>
    private void Log ( object text, int type, bool toDebug = false )
    {
      if ( debug && toDebug )
        Logger(text, type);
      else if ( !toDebug )
        Logger(text, type);
    }

    /// <summary>
    /// Program closing event
    /// </summary>
    /// <param name="sender">Sender<see cref="object"/></param>
    /// <param name="e">Args<see cref="FormClosingEventArgs"/></param>
    private void Main_FormClosing ( object sender, FormClosingEventArgs e )
    {
      Exit();
    }
  }
}
