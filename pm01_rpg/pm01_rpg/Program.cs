using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace pm01_rpg
{

    class Tech
    {
        [DllImport("msvcrt")]
        public static extern int _getch();

        public static string[] MerchPossibleWords = new string[5] {"Healing potion - restore: * HP, cost: @ SCORE", "Healing mana - restore: % MN, cost: @ SCORE",
            "Sword - give: # DMG cost: @ SCORE", "Combo potion - restore: + HP and MN, cost: @ SCORE", "Rage potion - give: # DMG restore: * HP cost: @ SCORE"};
        public static string[] MerchGoodAnswers = new string[3] { "Pleasure doing business for you))", "ALL Right!!!", "Thank you sir" };
        public static string[] MerchBadAnswers = new string[3] { "Don't come to me without MONEY!!!!", "Where your Money", "Hihihihihi.. Goy has no Money.. hihih" };


        public static int score = 0;

        public static int[,] defaultlvls = new int[5, 3] { { 250, 10, 10 }, { 425, 15, 10 }, { 600, 5, 15 }, { 900, 10, 10 }, { 1500, 20, 20 } };

        public static bool GameCont = true;
        public const int SX = 50;
        public const int Sy = 25;
        public static string ReadLine()
        {
            ConsoleKeyInfo key;
            StringBuilder sb = new StringBuilder();
            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
                sb.Append(key.KeyChar);
            return sb.ToString();
        }
    }
    abstract class Dialog
    {
        public int[,] effects;
        public string[,] answers;
        protected int variants, depth;
        public Dialog(int v, int d)
        {
            variants = v;
            depth = d;
            effects = new int[v * d, 3];
            answers = new string[depth, variants];
        }

        public abstract void Talk(Actor ch);
        protected abstract void GenerateDial();

    }

    class MerchanterD : Dialog
    {

        public MerchanterD(int a) : base(a, 1)
        {
            GenerateDial();
        }

        protected override void GenerateDial() // #(1) - dmg, *(2) - hp, @ - score, +(3) - hp & mn, %(4) - mn 
        {
            Random rng = new Random();
            int ef;
            for (int i = 0; i < variants; i++)
            {
                if (i != (variants - 1))
                {

                    answers[0, i] = Tech.MerchPossibleWords[rng.Next(0, 5)];
                    if (answers[0, i].Contains("*"))
                    {
                        ef = rng.Next(10, 25);
                        answers[0, i] = answers[0, i].Replace("*", ef.ToString());
                        effects[i, 0] = 2;
                        effects[i, 1] = ef;
                        ef = rng.Next(50, 100);
                        answers[0, i] = answers[0, i].Replace("@", ef.ToString());
                        effects[i, 2] = ef;
                    }
                    if (answers[0, i].Contains("#"))
                    {
                        ef = rng.Next(1, 6);
                        answers[0, i] = answers[0, i].Replace("#", ef.ToString());
                        effects[i, 0] = 1;
                        effects[i, 1] = ef;
                        ef = rng.Next(25, 50);
                        answers[0, i] = answers[0, i].Replace("@", ef.ToString());
                        effects[i, 2] = ef;
                    }
                    if (answers[0, i].Contains("+"))
                    {
                        ef = rng.Next(10, 25);
                        answers[0, i] = answers[0, i].Replace("+", ef.ToString());
                        effects[i, 0] = 3;
                        effects[i, 1] = ef;
                        ef = rng.Next(50, 100);
                        answers[0, i] = answers[0, i].Replace("@", ef.ToString());
                        effects[i, 2] = ef;
                    }
                    if (answers[0, i].Contains("%"))
                    {
                        ef = rng.Next(10, 25);
                        answers[0, i] = answers[0, i].Replace("%", ef.ToString());
                        effects[i, 0] = 4;
                        effects[i, 1] = ef;
                        ef = rng.Next(50, 100);
                        answers[0, i] = answers[0, i].Replace("@", ef.ToString());
                        effects[i, 2] = ef;
                    }
                }
                else
                {
                    answers[0, i] = "Exit";
                    effects[i, 2] = 0;
                }
            }
        }

        protected bool ApplyEffect(int ind, Actor ch)
        {
            bool rez = true;
            switch (effects[ind, 0])
            {
                case 1:
                    if (effects[ind, 2] <= Tech.score)
                        ch.Dmg += effects[ind, 1];
                    else
                        rez = false;
                    break;
                case 2:
                    if (effects[ind, 2] <= Tech.score)
                    {
                        if (ch.Hp + effects[ind, 1] <= ch.Mhp) ch.Hp += effects[ind, 1];
                        else if (ch.Hp + effects[ind, 1] >= ch.Mhp) ch.Hp = ch.Mhp;
                    }
                    else
                        rez = false;
                    break;
                case 3:
                    if (effects[ind, 2] <= Tech.score)
                    {
                        if (ch.Hp + effects[ind, 1] < ch.Mhp) ch.Hp += effects[ind, 1];
                        else if (ch.Hp + effects[ind, 1] >= ch.Mhp) ch.Hp = ch.Mhp;
                        if (ch.Mmn + effects[ind, 1] < ch.Mhp) ch.Mn += effects[ind, 1];
                        else if (ch.Mmn + effects[ind, 1] >= ch.Mhp) ch.Mn = ch.Mmn;
                    }
                    else
                        rez = false;
                    break;
                case 4:
                    if (effects[ind, 2] <= Tech.score)
                    {
                        if (ch.Mmn + effects[ind, 1] <= ch.Mhp) ch.Mn += effects[ind, 1];
                        else if (ch.Mmn + effects[ind, 1] >= ch.Mhp) ch.Mn = ch.Mmn;
                    }
                    else
                        rez = false;
                    break;
            }
            if (rez)
                Tech.score -= effects[ind, 2];
            return rez;
        }

        public override void Talk(Actor ch)
        {
            Random rng = new Random();
            Console.WriteLine();
            for (int i = 0; i < variants; i++)
            {
                Console.WriteLine(i + ") " + answers[0, i]);
            }
            Console.WriteLine("Choose...");
            ConsoleKeyInfo Cinp = Console.ReadKey();
            char inp = Cinp.KeyChar;
            int choose = int.Parse(inp.ToString());
            Console.WriteLine();
            if (ApplyEffect(choose, ch))
            {
                Console.WriteLine(Tech.MerchGoodAnswers[rng.Next(0, 3)]);
            }
            else
            {
                Console.WriteLine(Tech.MerchBadAnswers[rng.Next(0, 3)]);
            }
            Console.WriteLine();
            Console.ReadKey();
        }
    }

    class NPC : Actor
    {
        int type;
        Dialog dial;

        public NPC(int a, int b, int c, int x, int y, int type) : base(a, b, c, x, y)
        {
            this.type = type;
            Random rng = new Random();
            switch (this.type)
            {
                case 1:
                    av = "T";
                    dial = new MerchanterD(rng.Next(3, 5) + 1);
                    break;
            }

        }

        public override void Overlap(Actor ch)
        {
            this.dial.Talk(ch);
        }

        public override void Move(int x, int y)
        {

        }
    }

    abstract class Actor
    {
        public int askill { get; protected set; }


        protected int _hp;
        public int Mhp;
        protected int _dmg;
        protected int _mn;
        public int Mmn;
        public int x, y;
        public string av;

        public Actor(int a, int b, int c, int d, int e)
        {
            Mhp = a;
            _hp = Mhp;
            _dmg = b;
            Mmn = c;
            _mn = c;
            x = d;
            y = e;
        }

        public int Hp
        {
            get { return _hp; }
            set { _hp = value; }
        }

        public int Dmg
        {
            get { return _dmg; }
            set { _dmg = value; }
        }

        public int Mn
        {
            get { return _mn; }
            set { _mn = value; }
        }

        public abstract void Move(int x, int y);

        public abstract void Overlap(Actor ch);
    }

    class Enemy : Actor
    {
        public Enemy(int lvl, int x, int y) : base(1, 1, 1, x, y)
        {
            Random rng = new Random();
            switch (lvl)
            {
                case 1:
                    _hp = rng.Next(1, 10);
                    av = "E";
                    break;
                case 2:
                    _hp = rng.Next(11, 25);
                    av = "W";
                    break;
                case 3:
                    _hp = rng.Next(25, 55);
                    av = "Q";
                    break;
            }
        }

        public override void Move(int x, int y)
        {
            int wantx, wanty;
            if (this.x < x && x < Tech.SX)
            {
                // if (!(x - this.x == 1))
                //this.x++;
                wantx = 1;
            }
            else if (this.x > x && x > 0)
            {
                //  if (!(this.x - x == 1))
                wantx = -1;
            }
            else wantx = 0;

            if (this.y < y && y < Tech.Sy)
            {
                //    if (!(y - this.y == 1))
                wanty = 1;
            }
            else if (this.y > y && y > 0)
            {
                //   if (!(this.y - y == 1))
                wanty = -1;
            }
            else wanty = 0;

            Random rng = new Random();
            int turn = rng.Next(1, 3);
            if (turn == 1)
            {
                if (wantx == 1) this.x++;
                else if (wantx == -1) this.x--;
                else turn = 2;
            }
            if (turn == 2)
            {
                if (wanty == 1) this.y++;
                else if (wanty == -1) this.y--;
                else if (wantx != 0)
                {
                    if (wantx == 1) this.x++;
                    else if (wantx == -1) this.x--;
                }
            }
        }

        public override void Overlap(Actor ch)
        {
            int fdmg = _hp - ch.Dmg;
            if (fdmg > 0)
            {
                ch.Hp = ch.Hp - fdmg;
            }
            if (ch.Hp <= 0)
                Tech.GameCont = false;

            switch (av)
            {
                case "E":
                    Tech.score += 25;
                    break;
                case "W":
                    Tech.score += 40;
                    break;
                case "Q":
                    Tech.score += 75;
                    break;
            }
            //     Console.WriteLine("ENEMY!!!");//DEBUG
            //   Console.ReadLine();
        }
    }

    class Item : Actor
    {
        public Item(int lvl, int type, int x, int y) : base(1, 2, 3, x, y) // hp as type //dmg as value
        {
            Random rng = new Random();
            if (type == 1) // attac
            {
                _hp = 1;
                switch (lvl)
                {
                    case 1:
                        _dmg = rng.Next(1, 3);
                        av = "#";
                        break;
                    case 2:
                        _dmg = rng.Next(4, 5);
                        av = "@";
                        break;
                    case 3:
                        _dmg = rng.Next(6, 8);
                        av = "!";
                        break;
                }
            }
            else if (type == 2) // hp
            {
                _hp = 2;
                switch (lvl)
                {
                    case 1:
                        _dmg = rng.Next(1, 5);
                        av = "$";
                        break;
                    case 2:
                        _dmg = rng.Next(6, 10);
                        av = "%";
                        break;
                    case 3:
                        _dmg = rng.Next(11, 20);
                        av = "&";
                        break;
                }
            }
            else if (type == 3) // mana
            {
                _hp = 3;
                switch (lvl)
                {
                    case 1:
                        _dmg = rng.Next(5, 10);
                        av = "1";
                        break;
                    case 2:
                        _dmg = rng.Next(11, 20);
                        av = "2";
                        break;
                    case 3:
                        _dmg = rng.Next(22, 40);
                        av = "3";
                        break;
                }
            }
        }

        public override void Overlap(Actor ch)
        {
            switch (_hp)
            {
                case 1: //dmg
                    ch.Dmg = ch.Dmg + _dmg;
                    break;
                case 2: //hp
                    ch.Hp = ch.Hp + _dmg;
                    if (ch.Hp > ch.Mhp)
                        ch.Hp = ch.Mhp;
                    break;
                case 3: //mn
                    ch.Mn = ch.Mn + _dmg;
                    if (ch.Mn > ch.Mmn)
                        ch.Mn = ch.Mmn;
                    break;
            }
            //            Console.WriteLine("ITEM!!!"); // DEBUG
            //  Console.ReadLine();
        }

        public override void Move(int x, int y) { return; }
    }

    class Character : Actor
    {
        public int curlv = -1;

        public int[,] lvls = Tech.defaultlvls; // count str - count lvls, 1-st - required score, 2-nd - +hp, 3-rd - +mn

        public Character(int a, int b, int c, int x, int y) : base(a, b, c, x, y)
        {
            av = "0";
            askill = 0;
        }

        public void SetLvls(int[,] lv)
        {
            lvls = lv;
        }

        public void LvlUP(int lvl)
        {
            Mhp += lvls[lvl, 1];
            _hp = Mhp;
            Mmn += lvls[lvl, 2];
            _mn = Mmn;
            curlv = lvl;
        }

        public void Move(char i)
        {
            askill = 0;
            switch (i)
            {
                case 'w':
                    if (y - 1 > 0)
                    {
                        y--;
                        av = "^";
                    }
                    break;
                case 'a':
                    if (x - 1 > 0)
                    {
                        x--;
                        av = "<";
                    }
                    break;
                case 's':
                    if (y + 1 < Tech.Sy - 1)
                    {
                        y++;
                        av = "0";
                    }
                    break;
                case 'd':
                    if (x + 1 < Tech.SX - 1)
                    {
                        x++;
                        av = ">";
                    }
                    break;
                case ' ':
                    switch (av)
                    {
                        case "0":
                            if (y + 4 < Tech.Sy - 1)
                            {
                                y += 4;
                                _hp--;
                                if (_hp <= 0) Tech.GameCont = false;
                            }
                            else
                            {
                                y = Tech.Sy - 2;
                                _hp--;
                                if (_hp <= 0) Tech.GameCont = false;
                            }
                            break;
                        case "<":
                            if (x - 4 > 0)
                            {
                                x -= 4;
                                _hp--;
                                if (_hp <= 0) Tech.GameCont = false;
                            }
                            else
                            {
                                x = 1;
                                _hp--;
                                if (_hp <= 0) Tech.GameCont = false;
                            }
                            break;
                        case ">":
                            if (x + 4 < Tech.SX - 1)
                            {
                                x += 4;
                                _hp--;
                                if (_hp <= 0) Tech.GameCont = false;
                            }
                            else
                            {
                                x = Tech.SX - 2;
                                _hp--;
                                if (_hp <= 0) Tech.GameCont = false;
                            }
                            break;
                        case "^":
                            if (y - 4 > 0)
                            {
                                y -= 4;
                                _hp--;
                                if (_hp <= 0) Tech.GameCont = false;
                            }
                            else
                            {
                                y = 1;
                                _hp--;
                                if (_hp <= 0) Tech.GameCont = false;
                            }
                            break;
                    }
                    break;
                case 'r': //heal
                    if (_mn >= 10)
                    {
                        _hp += 25;
                        if (_hp > Mhp) _hp = Mhp;
                        askill = 1;
                        _mn -= 10;
                    }
                    break;
                case 'f': //square
                    if (_mn >= 25)
                    {
                        askill = 2;
                        _mn -= 25;
                    }
                    break;
                case 'c':
                    if (_mn >= 15) //ahead arrow
                    {
                        askill = 3;
                        _mn -= 15;
                    }
                    break;
            }
        }

        public override void Move(int x, int y) { return; }

        public override void Overlap(Actor ch)
        {
            //   Console.WriteLine("Char!!!"); // DEBUG
            //  Console.ReadLine();
        }
    }


    abstract class BFactory
    {
        public abstract Actor FactoryMethod(int s = 0);
    }

    class EFactory : BFactory
    {
        public override Actor FactoryMethod(int s = 0)
        {
            Random rng = new Random();
            if (s >= 1 && s <= 3)
            {
                return new Enemy(s, rng.Next(1, Tech.SX - 1), rng.Next(1, Tech.Sy - 1));
            }
            else
                return new Enemy(rng.Next(1, 4), rng.Next(1, Tech.SX - 1), rng.Next(1, Tech.Sy - 1));
        }
    }

    class IFactory : BFactory
    {
        public override Actor FactoryMethod(int s = 0)
        {
            Random rng = new Random();
            if (s >= 1 && s <= 3)
            {
                return new Item(rng.Next(1, 4), s, rng.Next(1, Tech.SX - 1), rng.Next(1, Tech.Sy - 1));
            }
            else
                return new Item(rng.Next(1, 4), rng.Next(1, 4), rng.Next(1, Tech.SX - 1), rng.Next(1, Tech.Sy - 1));

        }
    }

    class Coordinates
    {
        public int x, y;
        public Coordinates(int a, int b) { x = a; y = b; }
    }

    class Program
    {

        static List<Actor> Entities = new List<Actor>();
        static List<Coordinates> Cords = new List<Coordinates>();
        static List<NPC> NPS = new List<NPC>();

        static int Searcher(int x, int y, bool? inCh = true)
        {
            int result = -1;
            int i;
            if (inCh != null)
            {
                if (inCh == true) i = 0;
                else i = 1;
                for (; i < Entities.Count; i++)
                    if (x == Entities[i].x && y == Entities[i].y)
                    {
                        result = i;
                        return result;
                    }
            }
            else
            {
                for (i = 0; i < NPS.Count; i++)
                    if (x == NPS[i].x && y == NPS[i].y)
                    {
                        result = i;
                        return result;
                    }
            }
            return result;
        }

        static bool DSkill(int x, int y, Actor ch)
        {
            switch (ch.askill)
            {
                case 1:
                    return false; // heal don't draw
                    break;
                case 2:
                    if ((x == ch.x - 1 && y == ch.y - 1) || (x == ch.x && y == ch.y - 1) || (x == ch.x + 1 && y == ch.y - 1)
                        || (x == ch.x + 1 && y == ch.y) || (x == ch.x - 1 && y == ch.y) || (x == ch.x - 1 && y == ch.y + 1)
                        || (x == ch.x && y == ch.y + 1) || (x == ch.x + 1 && y == ch.y + 1))
                        if (x != 0 && y != 0 && x != Tech.SX - 1 && y != Tech.Sy - 1) return true;
                        else return false;
                    else return false;
                    break;
                case 3:
                    switch (ch.av)
                    {
                        case "0":
                            if (x == ch.x && ((y == ch.y + 1) || (y == ch.y + 2) || (y == ch.y + 3)))
                                if (x != 0 && y != 0 && x != Tech.SX - 1 && y != Tech.Sy - 1) return true;
                                else return false;
                            else return false;
                            break;
                        case "<":
                            if (y == ch.y && ((x == ch.x - 1) || (x == ch.x - 2) || (x == ch.x - 3)))
                                if (x != 0 && y != 0 && x != Tech.SX - 1 && y != Tech.Sy - 1) return true;
                                else return false;
                            else return false;
                            break;
                        case ">":
                            if (y == ch.y && ((x == ch.x + 1) || (x == ch.x + 2) || (x == ch.x + 3)))
                                if (x != 0 && y != 0 && x != Tech.SX - 1 && y != Tech.Sy - 1) return true;
                                else return false;
                            else return false;
                            break;
                        case "^":
                            if (x == ch.x && ((y == ch.y - 1) || (y == ch.y - 2) || (y == ch.y - 3)))
                                if (x != 0 && y != 0 && x != Tech.SX - 1 && y != Tech.Sy - 1) return true;
                                else return false;
                            else return false;
                            break;
                    }
                    return false;
                    break;
                default:
                    return false;
                    break;
            }
            return false;
        }

        static void Draw(bool skill = false)
        {
            Console.Clear();
            for (int y = 0; y < Tech.Sy; y++)
            {
                for (int x = 0; x < Tech.SX; x++)
                {
                    int loc = Searcher(x, y);
                    int npcl = Searcher(x, y, null);
                    if (x == 0 || x == Tech.SX - 1 || y == 0 || y == Tech.Sy - 1)
                    {
                        Console.Write("+");
                    }
                    else if (skill && Entities[0] != null && DSkill(x, y, Entities[0]))
                    {
                        Console.Write("*");
                        Cords.Add(new Coordinates(x, y));
                    }
                    else if (npcl != -1)
                    {
                        Console.Write(NPS[npcl].av);
                    }
                    else if (loc != -1)
                    {
                        Console.Write(Entities[loc].av);
                    }
                    else
                    {
                        Console.Write('.');
                    }
                }
                Character ch = Entities[0] as Character;
                if (y == 1) Console.Write(" HP: " + Entities[0].Hp);
                else if (y == 2) Console.Write(" DMG: " + Entities[0].Dmg);
                else if (y == 3) Console.Write(" MN: " + Entities[0].Mn);
                else if (y == 6) Console.Write(" SCORE: " + Tech.score);
                else if (y == 7) Console.Write(" CURRENT LVL: " + (ch.curlv + 1));
                else if (y == 8 && ch.curlv + 1 < 5) Console.Write(" SCORE TO NEXT LVL - " + ch.lvls[ch.curlv + 1, 0]);
                else if (y == 8 && ch.curlv + 1 == 5) Console.Write(" YOU GET MAX LVL ");
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            Character a;
            Console.WriteLine("Chose character class:");
            Console.WriteLine("1) Warrior (50 hp, 10 damage, 10 mana)");
            Console.WriteLine("2) Mage (10 hp, 20 damage, 100 mana)");
            Console.WriteLine("3) Berserk (10 hp, 25 damage, 5 mana)");
            int choose = int.Parse(Console.ReadLine());
            //custom progression for warrior
            int[,] wlv = new int[5, 3] { { 250, 5, 2 }, { 425, 10, 2 }, { 600, 15, 4 }, { 900, 10, 2 }, { 1500, 20, 15 } };
            //custom progression for mage
            int[,] mlv = new int[5, 3] { { 250, 5, 15 }, { 425, 10, 15 }, { 600, 15, 25 }, { 900, 10, 15 }, { 1500, 20, 30 } };
            //custom progression for berserk
            int[,] blv = new int[5, 3] { { 250, 2, 1 }, { 425, 2, 1 }, { 600, 15, 1 }, { 900, 3, 1 }, { 1500, 3, 1 } };

            switch (choose)
            {
                case 1:
                    a = new Character(50, 10, 10, 10, 10);
                    a.SetLvls(wlv);
                    break;
                case 2:
                    a = new Character(10, 20, 100, 10, 10);
                    a.SetLvls(mlv);
                    break;
                case 3:
                    a = new Character(10, 25, 5, 10, 10);
                    a.SetLvls(blv);
                    break;
                case 451:
                    a = new Character(100, 50, 100, 10, 10); // ubermensch
                    break;
                default:
                    a = new Character(5, 1, 5, 10, 10); // untermensh
                    break;
            }

            Console.Clear();
            Entities.Add(a);
            Entities.Add(new EFactory().FactoryMethod(1));
            Entities.Add(new EFactory().FactoryMethod());
            Entities.Add(new IFactory().FactoryMethod(1));
            NPS.Add(new NPC(10, 10, 10, 15, 15, 1));
            const int MAXITEM = 6;
            int tE = 4, tI = 3, itype = 0, ipow = 0, rsth = 0, rstm = 0, LVL = 0, citemcount = 1;
            bool sk, nlv = false; string enc = " ";
            Draw();
            while (Tech.GameCont)
            {
                enc = " "; itype = 0; ipow = 0; nlv = false;
                Cords.Clear();
                ConsoleKeyInfo Cinp = Console.ReadKey();
                char inp = Cinp.KeyChar;
                if (inp == 'w' || inp == 'a' || inp == 's' || inp == 'd' || inp == ' ' || inp == 'r' || inp == 'f' || inp == 'c')
                {
                    if (inp == 'w' || inp == 'a' || inp == 's' || inp == 'd' || inp == ' ')
                    {
                        a.Move(inp);
                        for (int i = 1; i < Entities.Count; i++)
                            Entities[i].Move(Entities[0].x, Entities[0].y);
                        sk = false;
                    }
                    else
                    {
                        sk = true;
                        a.Move(inp);
                    }
                    int npi = Searcher(Entities[0].x, Entities[0].y, null);
                    if (npi != -1)
                    {
                        NPS[npi].Overlap(Entities[0]);
                    }
                    int obj = Searcher(Entities[0].x, Entities[0].y, false);
                    if (obj != -1)
                    {
                        rsth = a.Hp;
                        rstm = a.Mn;
                        Entities[obj].Overlap(Entities[0]);
                        enc = Entities[obj].av;
                        if (!(enc == "E" || enc == "W" || enc == "Q"))
                        {
                            citemcount--;
                            enc = "item";
                            if (Entities[obj].Hp == 1)
                                ipow = Entities[obj].Dmg;
                            else if (Entities[obj].Hp == 2)
                            {
                                ipow = a.Hp - rsth;
                            }
                            else
                            {
                                ipow = a.Mn - rstm;
                            }
                        }
                        itype = Entities[obj].Hp;
                        Entities.RemoveAt(obj);
                    }
                    if (LVL < 5 && Tech.score >= a.lvls[LVL, 0] && a.Hp > 0)
                    {
                        a.LvlUP(LVL);
                        nlv = true;
                        LVL++;
                    }
                    Draw(sk);
                    if (!sk)
                        if (inp == ' ')
                            Console.WriteLine("YOU JUMP");
                        else
                            Console.WriteLine("YOU GO...");
                    else
                    {
                        Console.Write("YOU USED SKILL - ");
                        switch (inp)
                        {
                            case 'r':
                                Console.WriteLine("HEAL");
                                break;
                            case 'f':
                                Console.WriteLine("DEAD SHELL");
                                break;
                            case 'c':
                                Console.WriteLine("FORCE ARROW");
                                break;
                        }
                    }
                    if (enc == "E" || enc == "W" || enc == "Q")
                        Console.WriteLine("YOU HIT ENEMY!!!");
                    else if (enc == "item")
                    {
                        Console.Write("YOU PICK UP ITEM AND ");
                        switch (itype)
                        {
                            case 1: //dmg
                                Console.WriteLine("GIVE " + ipow + " DAMAGE");
                                break;
                            case 2: //hp
                                Console.WriteLine("RESTORE " + ipow + " HP");
                                break;
                            case 3:  //mn
                                Console.WriteLine("RESTORE " + ipow + " MANA");
                                break;
                        }
                    }
                    if (nlv)
                    {
                        Console.WriteLine("YOU GET NEW LEVEL!!!");
                    }
                    // Console.WriteLine(LVL);
                    if (sk)
                        for (int i = 0; i < Cords.Count; i++)
                        {
                            int ob = Searcher(Cords[i].x, Cords[i].y);
                            if (ob != -1)
                            {
                                Entities[ob].Hp -= Entities[0].Dmg;
                                if (Entities[ob].Hp <= 0) Entities.RemoveAt(ob);
                            }
                        }
                    tE--; tI--;
                    if (tI <= 0)
                    {
                        if (Entities.Count < 15 && citemcount < MAXITEM)
                        {
                            Entities.Add(new IFactory().FactoryMethod());
                            citemcount++;
                            tI = 3;
                        }
                    }
                    if (tE <= 0)
                    {
                        if (Entities.Count < 15) Entities.Add(new EFactory().FactoryMethod());
                        tE = 4;
                    }

                }
            }
            if (a.Hp > -11 && a.Hp <= 0)
                Console.WriteLine("YOU DIED IN BATTLE ;(");
            else if (a.Hp > -25 && a.Hp < -11)
                Console.WriteLine("YOU HAVE BEEN DESTROYED .. YOUR BODY WILL BE FOOD FOR RAVEN..");
            else
                Console.WriteLine("PIECES OF YOUR FLESH ARE SCATTERED FOR MANY MILES .. YOU HAD NO CHANCES..");
        }
    }
}
