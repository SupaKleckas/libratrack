Šio projekto tikslas – pagaminti lengvai prieinamą sistemą bibliotekų darbuotojams, kurioje būtų galima sekti bibliotekos inventorių internetu.
Šią platformą sudarys internetinis tinklapis, kuriuo galės naudotis bibliotekos darbuotojai bei sistemos administratoriai ir programavimo sąsaja. Platformoje bus trys naudotojų tipai: svečias, vartotojas ir administratorius.
Darbuotojas, norintis naudotis inventoriaus valdymo sistema, pirmiausia turės užsiregistruoti prie aplikacijos – tuomet taps vartotoju. Svečiai galės peržiūrėti pradinį puslapį ir užsiregistruoti. Naršyti knygų katalogą, pridėti, išimti knygas, redaguoti ir skaityti knygų aprašus galės užsiregistravę bibliotekos darbuotojai. Administratorius valdo bibliotekų ir jų skyrių pridėjimą bei pašalinimą, patvirtina bibliotekos darbuotojų registraciją. 

Biblioteka -> Skyrius -> Knyga

Funkciniai reikalavimai

Sistemos svečias galės:
1.	Peržiūrėti pradinį puslapį;
2.	Užsiregistruoti prie platformos, kad taptų vartotoju;
3.	Prisijungti prie internetinės aplikacijos.
   
Sistemos vartotojas galės:
1.	Atsijungti nuo internetinės aplikacijos;
2.	Peržiūrėti pradinį puslapį;
3.	Naršyti knygų katalogą;
4.	Pridėti naujas bibliotekos knygas į sistemą;
5.	Pašalinti bibliotekos knygas iš sistemos;
6.	Peržiūrėti knygos aprašą;
7.	Redaguoti knygų aprašą.
   
Sistemos administratorius galės:
1.	Pridėti naujas bibliotekas;
2.	Ištrinti biblioteką;
3.	Redaguoti bibliotekos informaciją;
4.	Pridėti bibliotekos skyrius;
5.	Ištrinti bibliotekos skyrius;
6.	Redaguoti bibliotekos skyriaus informaciją;
7.	Peržiūrėti vartotojų informaciją;
8.	Ištrinti vartotoją;
9.	Redaguoti vartotojo informaciją.

Sistemos architektūra
   
Sistemos sudedamosios dalys:
  1.	Kliento pusė (Front-end) – sudaryta naudojant Angular, HTML, CSS.
  2.	Serverio pusė (Back-end) – sudaryta naudojant ASP.NET Core. 
  3.	SQL duomenų bazė.

Sistemos talpinimui naudojamas Azure serveris.
![diagram drawio](https://github.com/SupaKleckas/libratrack/assets/100103272/86fef5e9-279f-41c7-a31e-c454f99b6fbf)

pav. 1 UML diegimo diagrama

