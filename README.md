# Preprosta predstavitev delovanja blockchaina z uporabo c# in .NET Core

## Build, edit

 Projekt Blokchain.sln odpri z Visual Stuido 2019
 Iz nuget se naložita paketa: EmbedIO, Newtonsoft.Json

## Opis programa in nastavitve

Struktura podatkov je v mapi Model. Vsebuje Blok in Transaction. Transakcija je iz treh polj: From, To, Amount

Bloki se kreirajo vsakih MINING_PERIOD sekund (nastavljeno 20 vsakih sekund) in če je število transakcij za obdelavo večje ali enako kot STEVILO_TRANSAKCIJ_V_BLOKU_MIN (privzeto 1)

Miner računa hash s funkcijo SHA256, zahtevnost računanja se nastavi s spremenljivko KOLIKO_NICEL_NA_ZACETKU_HASH. Privzeto je 0000. Pri tem je čas računanja nekaj sekund, število potrebnih ponovitev računanja: nekaj desettisoč. Za vsak konkreten blok je število računanj zapisano v polju: Nounce

Novi hash se računa iz: številke bloka, hash od prejšnjega bloka, TimeStamp bloka, težavnosti (nounce), merkleRootHash
v kodi: rowData = block.Index + block.PrevHash + block.TimeStamp.ToString() + nounce + merkleRootHash


## Post in get metodi za klic API

Ko program poženem lokalno, se naredi web server na privzetem naslovu: http://localhost:5449
Port se lahko nastavi drug.

Za debug in test sem uporabil Postman https://web.postman.co

Klic za dodajanje transakcije je POST, json:
POST http://localhost:5449/api/add
{"From":"Franc","To":"Janez","Amount":15}

Klic za pregled zadnjega bloka vrne json
GET http://localhost:5449/api/blocks/latest
Odgovor:
{
    "Index": 1,
    "TimeStamp": "2021-01-14T13:17:11.0289718+01:00",
    "Hash": "0000e22181132edbe8a672b9aceefd02417f057f6272c401cee6c3b33df02490",
    "PrevHash": "00007f21c20de7615e4037fbe1453650f2b2675632f4d077160a653b9a6b748d",
    "Nounce": 103767,
    "TransactionList": [
        {
            "From": "Franc",
            "To": "Janez",
            "Amount": 15
        }
    ]
}

Ostali klici
Pregled vseh blokov vrne array blokov od 0 naprej
GET http://localhost:5449/api/blocks

Klic za pregled točno določenega bloka X, 0 je prvi blok itd
GET http://localhost:5449/api/blocks/index/0







