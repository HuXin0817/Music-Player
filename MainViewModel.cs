using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using MusicPlayer.WPF.Base;
using Newtonsoft.Json;

namespace MusicPlayer.WPF;

public class MainViewModel : NotifyBase
{
    private MediaTimeline _timeline;

    private MediaPlayer _player = new() { Volume = 0.1 };

    public ObservableCollection<SongModel> SongList { get; set; }

    public MainViewModel()
    {
        SongList = new ObservableCollection<SongModel>(
            JsonConvert.DeserializeObject<List<SongModel>>(File.ReadAllText("songs.json")));

        _player.MediaOpened += (_, _) => { ProgressMax = _player.NaturalDuration.TimeSpan.TotalSeconds; };

        _player.MediaEnded += (_, _) => { DoNextCommand(null); };
    }

    private int _playMode;
    private string _playModeStr = "/icons/repeat.png";
    private string _playModeStrMouseover = "/icons/repeat_mouseover.png";

    public string PlayModeStr
    {
        get => _playModeStr;
        set
        {
            _playModeStr = value;
            Notify();
        }
    }

    public string PlayModeStr_mouseover
    {
        get => _playModeStrMouseover;
        set => _playModeStrMouseover = value;
    }

    public CommandBase PlayModeCommand
    {
        get => new CommandBase
        {
            DoExecute = _ =>
            {
                _playMode = (_playMode + 1) % 3;
                switch (_playMode)
                {
                    case 0:
                        PlayModeStr = "/icons/repeat.png";
                        PlayModeStr_mouseover = "/icons/repeat_mouseover.png";
                        break;
                    case 1:
                        PlayModeStr = "icons/repeatOnce.png";
                        PlayModeStr_mouseover = "icons/repeatOnce_mouseover.png";
                        break;
                    case 2:
                        PlayModeStr = "icons/shuffle.png";
                        PlayModeStr_mouseover = "icons/shuffle_mouseover.png";
                        break;
                }
            }
        };
    }

    private int _playState;

    private string _playStateStr = "/icons/play.png";

    private string _playStateStrMouseover = "/icons/play_mouseover.png";

    public string PlayStateStr
    {
        get => _playStateStr;
        set
        {
            _playStateStr = value;
            Notify();
        }
    }

    public string PlayStateStrMouseover
    {
        get => _playStateStrMouseover;
        set
        {
            _playStateStrMouseover = value;
            Notify();
        }
    }

    public CommandBase PlayStateCommand
    {
        get => new()
        {
            DoExecute = _ =>
            {
                _playState = (_playState + 1) % 2;
                switch (_playState)
                {
                    case 0:
                        PlayStateStr = "/icons/play.png";
                        PlayStateStrMouseover = "/icons/play_mouseover.png";
                        _player.Clock.Controller.Pause();
                        break;
                    case 1:
                        PlayStateStr = "/icons/pause.png";
                        PlayStateStrMouseover = "/icons/pause_mouseover.png";

                        if (_player.Clock != null && _player.Clock.IsPaused)
                        {
                            _player.Clock.Controller.Resume();
                        }
                        else
                        {
                            Play();
                        }

                        break;
                }
            }
        };
    }

    private int _playIndex;

    private void Play(SongModel songModel = null)
    {
        var currentSong = songModel ?? SongList[_playIndex];
        NowSongName = currentSong.SongName;
        NowSinger = currentSong.Singer;
        var exePath = AppDomain.CurrentDomain.BaseDirectory;
        var songPath = exePath + currentSong.SongPath;
        var uri = new Uri(songPath);
        _timeline = new MediaTimeline(uri);
        _timeline.CurrentTimeInvalidated += (_, _) =>
        {
            ProgressValue = _player.Position.TotalSeconds;
            var minute = ProgressValue / 60;
            var second = ProgressValue % 60;
            TimeStr = minute.ToString("00") + ":" + second.ToString("00") + "/";
            minute = ProgressMax / 60;
            second = ProgressMax % 60;
            TimeStr += minute.ToString("00") + ":" + second.ToString("00");
        };
        _player.Clock = _timeline.CreateClock(true) as MediaClock;
        _player.Clock?.Controller?.Begin();
    }

    private string _nowSongName = "暂无歌曲播放";

    public string NowSongName
    {
        get => _nowSongName;
        set
        {
            _nowSongName = value;
            Notify();
        }
    }

    private string _nowSinger = "未知歌手";

    public string NowSinger
    {
        get => _nowSinger;
        set
        {
            _nowSinger = value;
            Notify();
        }
    }

    private double _progressMax = 100.0;

    public double ProgressMax
    {
        get => _progressMax;
        set
        {
            _progressMax = value;
            Notify();
        }
    }

    private double _progressValue;

    public double ProgressValue
    {
        get => _progressValue;
        set
        {
            _progressValue = value;
            Notify();
        }
    }

    private string _timeStr = "00:00/00:00";

    public string TimeStr
    {
        get => _timeStr;
        set
        {
            _timeStr = value;
            Notify();
        }
    }

    public CommandBase PreviousCommand
    {
        get => new()
        {
            DoExecute = _ =>
            {
                _playIndex = (_playIndex + SongList.Count - 1) % SongList.Count;
                Play();
                PlayStateStr = "/icons/pause.png";
                PlayStateStrMouseover = "/icons/pause_mouseover.png";
                _playState = 1;
            }
        };
    }

    public CommandBase NextCommand
    {
        get => new()
        {
            DoExecute = DoNextCommand
        };
    }

    private void DoNextCommand(object _)
    {
        switch (_playMode)
        {
            case 0:
            case 1:
                _playIndex = (_playIndex + 1) % SongList.Count;
                break;
            case 2:
                var oldIdx = _playIndex;
                do
                {
                    _playIndex = _random.Next(0, SongList.Count - 1);
                } while (_playIndex == oldIdx);
                break;
        }

        PlayStateStr = "/icons/pause.png";
        PlayStateStrMouseover = "/icons/pause_mouseover.png";
        _playState = 1;

        Play();
    }

    private Random _random = new();

    public CommandBase DoubleClickPlayCommand
    {
        get => new()
        {
            DoExecute = obj =>
            {
                Play(obj as SongModel);
                PlayStateStr = "/icons/pause.png";
                PlayStateStrMouseover = "/icons/pause_mouseover.png";
                _playState = 1;
            }
        };
    }

    private double _listHigh;

    public double ListHigh
    {
        get => _listHigh;
        set
        {
            _listHigh = value;
            Notify();
        }
    }

    public CommandBase ListHeightCommand
    {
        get => new()
        {
            DoExecute = _ => { ListHigh = 310 - ListHigh; }
        };
    }
}