namespace XORCipher

open System
open System.Text

type public XOR_Cipher() =
    static member GetRandomKey(key: int, length: int) =
        let gamma = StringBuilder()
        let rnd = Random(key)
        for i = 0 to length do gamma.Append(string(char(rnd.Next(35, 126)))) |> ignore
        gamma.ToString()

    member private this.GetRepeatKey (s: string) (n: int) =
        let r = StringBuilder(s)
        while r.Length < n do r.Append(r) |> ignore
        r.ToString().Substring(0, n)

    member private this.Cipher (text: string) (secretKey: string) =
        let currentKey = this.GetRepeatKey secretKey text.Length
        let res = StringBuilder()
        for i = 0 to text.Length - 1 do res.Append(string(char(int(text[i]) ^^^ int(currentKey[i])))) |> ignore
        res.ToString()

    member public this.Encrypt(plainText: string, password: string) = this.Cipher plainText password

    member public this.Decrypt(encryptedText: string, password: string) = this.Cipher encryptedText password