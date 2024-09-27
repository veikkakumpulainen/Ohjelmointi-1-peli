using System.Collections.Generic;
using Jypeli;
using Jypeli.Widgets;
using Timer = Jypeli.Timer;
using Vector = Jypeli.Vector;

namespace Approt;

/// @author kumpuvex
/// @version 22.11.2023
/// <summary>
/// Tarkoitus kerätä kaikki leimat appropassiin ennen ajan loppumista.
/// </summary>
public class Approt : PhysicsGame
{
    private PhysicsObject pelaaja;
    private IntMeter elamalaskuri;
    private IntMeter leimalaskuri;
    private Timer aikalaskuri;
    private ProgressBar leimapalkki;
    private Image baari0k = LoadImage("baari0kuva");
    private Image baari1k = LoadImage("baari1kuva");
    private Image baari2k = LoadImage("baari2kuva");
    private Image baari3k = LoadImage("baari3kuva");
    private Image baari4k = LoadImage("baari4kuva");
    private Image apteekkik = LoadImage("apteekkikuva");
    private Image yokerhok = LoadImage("yokerhokuva");
    private Image appropassi0 = LoadImage("appropassi0");
    private Image appropassi5 = LoadImage("appropassi5");
    private Image appropassi6 = LoadImage("appropassi6");
    private List<PhysicsObject> viholliset = new List<PhysicsObject>();
    

    /// <summary>
    /// Kutsutaan aliohjelmia, jotta saadaan peliin kenttä, ohjaimet ja laskurit
    /// </summary>
    public override void Begin()
    {
        LuoKentta();
        AsetaOhjaimet();
        LuoElamalaskuri();
        LuoLeimalaskuri();
        LuoAikalaskuri();
    }

    
    /// <summary>
    /// Luodaan kentän rajat, rakennukset, pelaaja, vihollinen ja CollisionHandlerit
    /// </summary>
    public void LuoKentta()
    {
        Level.BackgroundColor = Color.White;
        PhysicsObject vasenReuna = Level.CreateLeftBorder();
        vasenReuna.Restitution = 1.0;
        vasenReuna.IsVisible = false;

        PhysicsObject oikeaReuna = Level.CreateRightBorder();
        oikeaReuna.Restitution = 1.0;
        oikeaReuna.IsVisible = false;

        PhysicsObject ylaReuna = Level.CreateTopBorder();
        ylaReuna.Restitution = 1.0;
        ylaReuna.IsVisible = false;

        PhysicsObject alaReuna = Level.CreateBottomBorder();
        alaReuna.Restitution = 1.0;
        alaReuna.IsVisible = false;
        Camera.ZoomToLevel();

        string leima = "leima";
        
        PhysicsObject apteekki = LuoRakennus(this, 150, 150, Shape.Rectangle, Level.Left + 10, Level.Top + 10, Color.Green, apteekkik, null);
        PhysicsObject baari0 = LuoRakennus(this, 150, 150, Shape.Rectangle, Level.Right + 10, Level.Top + 10, Color.Brown, baari0k, leima) ;
        PhysicsObject baari1 = LuoRakennus(this, 150, 150, Shape.Rectangle, Level.Right + 10, 0, Color.LightGray, baari1k, leima) ;
        PhysicsObject baari2 = LuoRakennus(this, 150, 150, Shape.Rectangle, 0, Level.Bottom + 10, Color.LightPink, baari2k, leima) ;
        PhysicsObject baari3 = LuoRakennus(this, 150, 150, Shape.Rectangle, Level.Left + 10, Level.Bottom + 10, Color.Red, baari3k, leima) ;
        PhysicsObject baari4 = LuoRakennus(this, 150, 150, Shape.Rectangle, Level.Left + 10, 0, Color.Blue, baari4k, leima) ;
        PhysicsObject yokerho = LuoRakennus(this, 150, 150, Shape.Rectangle, Level.Right + 10, Level.Bottom + 10, Color.DarkViolet, yokerhok, null) ;
        
        pelaaja = LuoPelaaja(this, 50, Color.Yellow, 0, 0);
        PhysicsObject vihollinen = LuoVihollinen(this, 30, Color.Red, 0, 100);
        
        AddCollisionHandler(pelaaja, "pahis", PelaajaOsuuViholliseen);
        AddCollisionHandler(pelaaja, "leima", PelaajaKeraaLeiman);
        AddCollisionHandler(pelaaja, apteekki, PelaajaSaaElamia);
        AddCollisionHandler(pelaaja, yokerho, PelaajaVoittaaPelin);
        

    }

    
    /// <summary>
    /// Luo peliin annetuilla parametreilla rakennuksen
    /// </summary>
    /// <param name="peli">Peli, johon rakennus lisataan</param>
    /// <param name="x">Rakennuksen leveys</param>
    /// <param name="y">Rakennuksen korkeus</param>
    /// <param name="muoto">Minkä muotoinen rakennus halutaan</param>
    /// <param name="xKord">Rakennuksen x-koordinaatti</param>
    /// <param name="yKord">Rakennuksen y-koordinaatti</param>
    /// <param name="vari">Rakennuksen vari</param>
    /// <param name="kuvanNimi">Kuva rakennuksen ulkoasulle</param>
    /// <param name="id">Maaritys CollisionHandlereitä varten</param>
    /// <returns>Palauttaa rakennuksen</returns>
    private PhysicsObject LuoRakennus(Game peli, int x, int y, Shape muoto, double xKord, double yKord, Color vari, Image kuvanNimi, string id)
    {
        PhysicsObject rakennus = new PhysicsObject(x, y, muoto, xKord, yKord);
        rakennus.Color = vari;
        rakennus.Mass = 100000;
        rakennus.Restitution = 1;
        rakennus.Tag = id;
        rakennus.Image = kuvanNimi;
        rakennus.CanRotate = false;
        peli.Add(rakennus);
        return rakennus;
    }
    
    
    /// <summary>
    /// Luo peliin pelaajan
    /// </summary>
    /// <param name="peli">Peli, johon lisataan</param>
    /// <param name="halkaisija">pelaajan halkaisija</param>
    /// <param name="vari">pelaajan vari</param>
    /// <param name="x">Pelaajan x-koordinaatti alussa</param>
    /// <param name="y">Pelaajan y-koordinaatti alussa</param>
    /// <returns>Palauttaa pelaajan</returns>
    private PhysicsObject LuoPelaaja(Game peli, double halkaisija, Color vari, double x, double y)
    {
        PhysicsObject pallo = new PhysicsObject(halkaisija, halkaisija, Shape.Circle, x, y);
        pallo.Color = vari;
        pallo.Image = LoadImage("pelaajakuva");
        peli.Add(pallo);
        return pallo;
    }
    
    
    /// <summary>
    /// Luo peliin vihollisen
    /// </summary>
    /// <param name="peli">Peli, johon luodaan</param>
    /// <param name="halkaisija">Vihollisen halkaisija</param>
    /// <param name="vari">Vihollisen vari</param>
    /// <param name="x">Vihollisen x-koordinaatti alussa</param>
    /// <param name="y">Vihollisen y-koordinaatti alussa</param>
    /// <returns>Palauttaa vihollisen</returns>
    private PhysicsObject LuoVihollinen(Game peli, double halkaisija, Color vari, double x, double y)
    {
        PhysicsObject pallo = new PhysicsObject(halkaisija, halkaisija, Shape.Circle, x, y);
        pallo.Color = vari;
        pallo.Tag = "pahis";
        int a = RandomGen.NextInt(-200, 200);
        int b = RandomGen.NextInt(-200, 200);
        pallo.Hit(new Vector(a, b));
        pallo.Restitution = 1;
        pallo.Image = LoadImage("vihollinen");
        peli.Add(pallo);
        viholliset.Add(pallo);
        return pallo;
    }
    
    
    /// <summary>
    /// Määritetään ohjaimet, jolla pelaajaa voi liikuttaa
    /// </summary>
    private void AsetaOhjaimet()
    {
        Keyboard.Listen(Key.Up, ButtonState.Pressed, AsetaNopeus, "Liikuta pelaajaa ylös", pelaaja, new Vector(0, 200));
        Keyboard.Listen(Key.Down, ButtonState.Pressed, AsetaNopeus, "Liikuta pelaajaa alas", pelaaja, new Vector(0, -200));
        Keyboard.Listen(Key.Right, ButtonState.Pressed, AsetaNopeus, "Liikuta pelaajaa oikealle", pelaaja, new Vector(200, 0));
        Keyboard.Listen(Key.Left, ButtonState.Pressed, AsetaNopeus, "Liikuta pelaajaa ylös", pelaaja, new Vector(-200, 0));

        Keyboard.Listen(Key.Enter, ButtonState.Pressed, AloitaAlusta, "Aloita peli alusta");
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }
    
    
    /// <summary>
    /// Estää pallon liimautumisen seiniin ja luo pelaajaan liikkeen
    /// </summary>
    /// <param name="pallo">Kohde, jota liikutetaan</param>
    /// <param name="nopeus">Määrittää nopeuden ja suunnan pallon liikkeelle</param>
    void AsetaNopeus(PhysicsObject pallo, Vector nopeus)
    {
        if ((nopeus.Y < 0) && (pelaaja.Bottom < Level.Bottom))
        {
            pallo.Velocity = Vector.Zero;
            return;
        }
        if ((nopeus.Y > 0) && (pelaaja.Top > Level.Top))
        {
            pallo.Velocity = Vector.Zero;
            return;
        }

        pelaaja.Velocity = nopeus;
    }

    
    /// <summary>
    /// Kun pelaaja osuu viholliseen, luo uuden vihollisen ja laskee elämälaskuria yhdellä elämällä
    /// </summary>
    /// <param name="tormaaja">Pelaaja, jolla törmätään</param>
    /// <param name="kohde">Vihollinen, johon törmättiin</param>
    private void PelaajaOsuuViholliseen(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        LuoVihollinen(this, 30, Color.Red, 0, 100);
        elamalaskuri.Value -= 1;
    }

    
    /// <summary>
    /// Kun pelaaja kerää leiman, appropassiin tulee uusi merkintä ja vihollisia syntyy lisää
    /// </summary>
    /// <param name="tormaaja">Pelaaja</param>
    /// <param name="kohde">Rakennus, johon törmättäessä saadaan uusi leima</param>
    private void PelaajaKeraaLeiman(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        while (leimalaskuri.Value < 5)
        {
            leimalaskuri.Value++;
            leimapalkki.Image = LoadImage("appropassi" + leimalaskuri.Value);   
            kohde.Destroy();
            for (int k = 0; k < 2; k++)
            {
                LuoVihollinen(this, 30, Color.Red, 0, 0);
            }
            break;
        }
        if (leimalaskuri.Value == 5)
        {
            leimapalkki.Image = appropassi5;
            for (int i = 0; i < 20; i++)
            {
                LuoVihollinen(this, 30, Color.Red, 0, 0);
            }
            
        }
        
    }

    
    /// <summary>
    /// Kun pelaaja törmää apteekkiin, elämälaskuri kasvaa yhdellä
    /// </summary>
    /// <param name="tormaaja">Pelaaja</param>
    /// <param name="kohde">Apteekki, josta elämän saa</param>
    private void PelaajaSaaElamia(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        elamalaskuri.Value += 1;
    }
    
    
    /// <summary>
    /// Luodaan elämälaskuri peliin
    /// </summary>
    private void LuoElamalaskuri()
    {
        elamalaskuri = new IntMeter(3);
        elamalaskuri.MaxValue = 3;
        elamalaskuri.LowerLimit += ElamaLoppui;

        ProgressBar elamapalkki = new ProgressBar(150, 20);
        elamapalkki.X = 250;
        elamapalkki.Y = Screen.Top - 20;
        elamapalkki.Color = Color.LightPink;
        elamapalkki.BindTo(elamalaskuri);
        Add(elamapalkki);
    }

    
    /// <summary>
    /// Jos elämät loppuu, peli alkaa alusta
    /// </summary>
    private void ElamaLoppui()
    {
        MessageDisplay.Add("Gameover :( Try Again");
        AloitaAlusta();
    }

    
    /// <summary>
    /// Luodaan leimalaskuri
    /// </summary>
    private void LuoLeimalaskuri()
    {
        leimalaskuri = new IntMeter(0);
        leimalaskuri.MaxValue = 6;
        leimapalkki = new ProgressBar(200, 150);
        leimapalkki.X = 0;
        leimapalkki.Y = Screen.Top - 80;
        leimapalkki.Image = appropassi0;
        leimapalkki.BindTo(leimalaskuri);
        Add(leimapalkki);
    }
    
    
    /// <summary>
    /// Jos pelaaja saa kerättyä kaikki leimat, peli päättyy.
    /// </summary>
    /// <param name="tormaaja">Pelaaja</param>
    /// <param name="kohde">Viimeinen kohde, josta pitää leima kerätä</param>
    private void PelaajaVoittaaPelin(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        
        if (leimalaskuri.Value == 5)
        {
            aikalaskuri.Pause();
            double aikaaKulunut = aikalaskuri.SecondCounter.Value;
            MessageDisplay.BackgroundColor = Color.Black;
            MessageDisplay.TextColor = Color.White;
            MessageDisplay.Add($"Onneksi olkoon, appropassisi on täynnä, aikaa siihen kuluui {aikaaKulunut:0.00}, voit aloittaa pelin alusta painamalla Enteriä");
            leimalaskuri.Value += 1;
            if (leimalaskuri.Value == 6)
            {
                leimapalkki.Y = 0;
                leimapalkki.Image = appropassi6;
                foreach (var alkio in viholliset)
                {
                    alkio.Destroy();
                }
                kohde.Destroy();
            }
        }
    }


    /// <summary>
    /// Luodaan aikalaskuri
    /// </summary>
    private void LuoAikalaskuri()
    {
        aikalaskuri = new Timer();
        aikalaskuri.Interval = 30;
        aikalaskuri.Timeout += AikaLoppui;
        aikalaskuri.Start(1);

        Label aikanaytto = new Label();
        aikanaytto.TextColor = Color.Black;
        aikanaytto.X = -300;
        aikanaytto.Y = Screen.Top - 40;
        aikanaytto.DecimalPlaces = 1;
        aikanaytto.BindTo(aikalaskuri.SecondCounter);
        Add(aikanaytto);
    }

    
    /// <summary>
    /// Jos aika loppuu, peli alkaa alusta
    /// </summary>
    private void AikaLoppui()
    {
        AloitaAlusta();
    }
    
    
    /// <summary>
    /// Nollaa pelin ja aloittaa alusta
    /// </summary>
    void AloitaAlusta()
    
    {
        ClearAll();
        LuoKentta();
        AsetaOhjaimet();
        LuoElamalaskuri();
        LuoLeimalaskuri();
        LuoAikalaskuri();
        leimalaskuri.Value = 0;
        Run();
    }
    

    
    
}   