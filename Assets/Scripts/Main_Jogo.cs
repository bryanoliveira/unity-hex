﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Main_Jogo : MonoBehaviour
{

    // ---- CONSTS
    private const int ESTADO_JOGANDO = 0;
    private const int ESTADO_FIM = 1;
    private const int ESTADO_RECORDES = 2;
    private const int cameraDist = -10;

    private const float velocidadeFoco = 3f;
    // ---- CONSTS

    public Hexagono_Controlador foco;
    public Hexagono_Controlador atual;

    // ---- COISAS DO TUTORIAL
    private bool emTutorial = false;
    private bool esperandoToque = true;
    private bool esperandoAcao = true;
    // ---- COISAS DO TUTORIAL

    [SerializeField]
    private Animator animPontos;
    [SerializeField]
    private Animator animAviso;
    [SerializeField]
    private Animator animFadeIn;
    [SerializeField]
    private Animator animBackground;

    [SerializeField]
    private SpriteRenderer spriteBackground;

    [SerializeField]
    private InputField txtNomeLocal;

    [SerializeField]
    private Text txtAviso;
    [SerializeField]
    private Text txtPontos;
    [SerializeField]
    private Text txtRecorde;
    [SerializeField]
    private Text txtNomeRecorde;
    [SerializeField]
    private Text txtBotaoVibracao;

    public static int estado = 0;
    public static int pontos;
    private static int proxRecordeIndice;
    private static int proxRecorde;

    //[SerializeField]
    //private string hexUrl;

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private GameObject paginaRecordes;
    [SerializeField]
    private GameObject paginaInsiraNome;
    [SerializeField]
    private GameObject paginaPausa;
    [SerializeField]
    private GameObject botaoDeslogar;
    [SerializeField]
    private GameObject botaoPausa;
    [SerializeField]
    private GameObject botaoTela;
    [SerializeField]
    private GameObject prefabRecorde;
    [SerializeField]
    private GameObject prefabHex;

    [SerializeField]
    private Transform scrollRecordes;
    [SerializeField]
    private Transform scrollPause;

    private List<Recorde> tabelaRecordes;

    [SerializeField]
    private UnityStandardAssets.ImageEffects.ColorCorrectionCurves cc;

    void Awake()
    {
        SetTabela();

        // prepara o primeiro hexagono
        StartCoroutine(Foca());

        // inicializa vetor de cores uma única vez
        Hexagono_Controlador.IniciaCores();

        // inicia o tutorial, se necessário
        if (PlayerPrefs.GetInt("tutorial") == 0)
        {
            StartCoroutine(Tutorial());
        }

        txtNomeLocal.text = PlayerPrefs.GetString("ultimoRecordista");

        // liga a vibração se for pra ligar
#if UNITY_ANDROID
        SetVibracao(PlayerPrefs.GetInt("vibracao") == 1);
#endif
    }

    private void Update()
    {
        if (estado == 0)
        { // durante o jogo
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Toque();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                paginaPausa.SetActive(Time.timeScale != 0);
                Pausa(Time.timeScale != 0);
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                ZoomOut();
            }
        }
        else if (estado == 1)
        { // fim de jogo
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Reiniciar();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                MostraRecordesFim();
            }
        }
        else if (estado == 2)
        { // mostrando recordes
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
            {
                Reiniciar();
            }
        }
    }

    public void SetTabela()
    {
        tabelaRecordes = new List<Recorde>();
        for (int i = 0; PlayerPrefs.HasKey("recordeLocal" + i); i++)
        {
            tabelaRecordes.Add(new Recorde(PlayerPrefs.GetString("nomeRecordeLocal" + i), PlayerPrefs.GetInt("recordeLocal" + i)));

        }
        proxRecordeIndice = tabelaRecordes.Count;
        ProximoDesafio();
    }

    public void AddPontos()
    {
        pontos++;
        animPontos.SetTrigger("up");
        if (proxRecordeIndice >= 0)
        {
            if (pontos > proxRecorde && !emTutorial)
            {
                Avisa(tabelaRecordes[proxRecordeIndice].nome + " superado!");
                ProximoDesafio();
            }
        }

        txtPontos.text = pontos.ToString();

        // adiciona nova cor pra aumentar dificuldade
        if (pontos % 10 == 0)
        {
            Hexagono_Controlador.AddCor();
        }
    }
    private void ProximoDesafio()
    {
        proxRecordeIndice--;
        if (proxRecordeIndice >= 0)
        {
            txtNomeRecorde.text = tabelaRecordes[proxRecordeIndice].nome;
            proxRecorde = tabelaRecordes[proxRecordeIndice].score;
            txtRecorde.text = proxRecorde.ToString();
        }
        else
        {
            txtRecorde.text = "novo";
            txtNomeRecorde.text = "recorde";
        }
    }

    public void ResetaPontos()
    {
        pontos = 1;
        txtPontos.text = "1";
        proxRecordeIndice = tabelaRecordes.Count;
        ProximoDesafio();
    }

    public void Toque()
    {
        if (emTutorial)
            esperandoToque = false;
        foco = atual;
        foco.Liga();
        StartCoroutine(foco.GiraEmVolta());
        StartCoroutine(Foca());
    }

    private IEnumerator Foca()
    {
        while (Vector3.Distance(transform.position, foco.transform.position) > 0)
        {
            Vector3 novaPos = Vector3.MoveTowards(transform.position, foco.transform.position, 1);
            transform.position = new Vector3(novaPos.x, novaPos.y, cameraDist);
            yield return new WaitForSeconds(velocidadeFoco * Time.deltaTime);
        }
    }

    public void Liga(GameObject oque)
    {
        oque.SetActive(true);
    }
    public void Desliga(GameObject oque)
    {
        oque.SetActive(false);
    }

    public void Pausa(bool sim)
    {
        if (sim)
        {
            Time.timeScale = 0;
            MostraRecordes(scrollPause);
        }
        else
        {
            if (emTutorial)
            {
                Time.timeScale = 0.5f;
            }
            else
                Time.timeScale = 1;
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        ;
    }

    public void ZoomOut()
    {
        if (emTutorial)
            esperandoAcao = false;
        StartCoroutine(ZoomOutRoutine());
    }

    private IEnumerator ZoomOutRoutine()
    {
        while (cam.orthographicSize < 10)
        {
            cam.orthographicSize++;
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(1);
        while (cam.orthographicSize > 5)
        {
            cam.orthographicSize--;
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void Avisa(string oque)
    {
        txtAviso.text = oque;
        animAviso.SetTrigger("mostra");
    }
    public void Avisa(string oque, bool aparece)
    {
        if (aparece)
        { // se está aparecendo, faz fadeIn e espera vir instrução para sair
            txtAviso.text = oque;
            animAviso.SetTrigger("aparece");
        }
        else
        { // instrução para sair
            txtAviso.text = "Ok!";
            animAviso.SetTrigger("desaparece");
        }
    }

    public void Reiniciar()
    {
        ResetaPontos();
        StartCoroutine(ReiniciaRoutine());
    }
    private IEnumerator ReiniciaRoutine()
    {
        Time.timeScale = 1;
        animFadeIn.SetTrigger("FadeOut");
        yield return new WaitForSeconds(0.52f);
        estado = ESTADO_JOGANDO;
        Hexagono_Controlador.Reiniciar();
        SceneManager.LoadScene(0);
    }

    private IEnumerator Tutorial()
    {
        // desabilita poluição da gui e prepara
        emTutorial = true;
        Time.timeScale = 0.5f;
        botaoPausa.SetActive(false);
        txtPontos.gameObject.SetActive(false);
        txtRecorde.transform.parent.gameObject.SetActive(false);
        ResetaPontos();

        // apresenta como jogar
        yield return StartCoroutine(PassoTutorial("Toque na tela para ligar o hexágono em foco"));

        // apresenta objetivo
        yield return StartCoroutine(PassoTutorial("Se os hexágonos ligados forem da mesma cor, você pontua"));
        txtPontos.gameObject.SetActive(true);
        // espera a pessoa pontuar
        Avisa("Ligue hexágonos da mesma cor!\nFaça 5 pontos!", true);
        while (pontos < 5)
        {
            yield return null;
        }
        Avisa("", false);
        yield return new WaitForSeconds(0.5f);


        // apresenta os pontos
        txtPontos.gameObject.SetActive(true);
        yield return StartCoroutine(PassoTutorial("Seus pontos aparecem lá em cima"));

        // apresenta os recordes e coloca um recorde temporario
        txtRecorde.transform.parent.gameObject.SetActive(true);
        yield return StartCoroutine(PassoTutorial("Os pontos que faltam para bater o recorde aparecem abaixo"));
        yield return new WaitForSeconds(0.5f);

        // apresenta os coringas
        Hexagono_Controlador.TodosSaoCoringas(true);
        yield return StartCoroutine(PassoTutorial("Hexagonos que mudam de cor são coringas"));
        Hexagono_Controlador.TodosSaoCoringas(false);

        // apresenta o zoomout
        Avisa("Toque na parte superior da tela para ter uma visão geral", true);
        esperandoAcao = true;
        while (esperandoAcao)
            yield return null;
        Avisa("", false);
        yield return new WaitForSeconds(0.5f);

        // apresenta a pausa
        botaoPausa.SetActive(true);
        Avisa("Toque nas barras lá em cima para pausar", true);
        while (Time.timeScale != 0)
            yield return null;
        Avisa("", false);
        yield return new WaitForSeconds(0.5f);

        // finaliza
        Avisa("Tutorial completo!");

        // inserir conquista

        emTutorial = false;
        PlayerPrefs.SetInt("tutorial", 1);
        // -- essa parte é temporaria pelo comentario de cima
        yield return new WaitForSeconds(1.5f);
        Reiniciar();
        // --
    }

    private IEnumerator PassoTutorial(string texto)
    {
        Avisa(texto, true);
        esperandoToque = true;
        yield return new WaitForSeconds(2f);
        while (esperandoToque)
            yield return null;
        Avisa("", false);
        yield return new WaitForSeconds(0.5f);
    }

    public void Perde()
    {
        BGFLash(Color.white);
        if (pontos > 2 && !emTutorial)
        {
            botaoTela.SetActive(false);
            StartCoroutine(DiminuiEFinaliza());
        }
        else
        {
            ResetaPontos();
        }
    }

    private IEnumerator DiminuiEFinaliza()
    {
        while (cc.saturation > 0.2)
        {
            cc.saturation -= 0.2f;
            yield return new WaitForSeconds(0.1f);
        }

        paginaInsiraNome.SetActive(true);
    }

    private void SalvaRecordeLocal()
    {
        int recordeAtual = pontos;
        string nomeRecordeAtual = txtNomeLocal.text.ToLower();
        PlayerPrefs.SetString("ultimoRecordista", nomeRecordeAtual);
        for (int i = 0; i < 10; i++)
        {
            // ordena a tabela de recordes na insersão
            if (PlayerPrefs.HasKey("recordeLocal" + i))
            {
                // se o usuário já está na lista
                if (PlayerPrefs.GetString("nomeRecordeLocal" + i) == nomeRecordeAtual)
                {
                    // se a pontuação é maior
                    if (PlayerPrefs.GetInt("recordeLocal" + i) < recordeAtual)
                    {
                        // substitui a pontuação
                        PlayerPrefs.SetInt("recordeLocal" + i, recordeAtual);
                    }
                    // senão só ignora
                    break;
                }
                if (PlayerPrefs.GetInt("recordeLocal" + i) < recordeAtual)
                {
                    // se o recorde atual é maior, coloca ele na lista e carrega o 
                    // recorde menor pra próxima posição
                    int tempRecorde = recordeAtual;
                    string tempNome = nomeRecordeAtual;
                    recordeAtual = PlayerPrefs.GetInt("recordeLocal" + i);
                    nomeRecordeAtual = PlayerPrefs.GetString("nomeRecordeLocal" + i);
                    PlayerPrefs.SetInt("recordeLocal" + i, tempRecorde);
                    PlayerPrefs.SetString("nomeRecordeLocal" + i, tempNome);
                }
            }
            else
            {
                // se chegamos numa posição ainda não ocupada, ocupa ela
                PlayerPrefs.SetInt("recordeLocal" + i, recordeAtual);
                PlayerPrefs.SetString("nomeRecordeLocal" + i, nomeRecordeAtual);
                break;
            }
        }
    }

    public void MostraRecordesFim()
    {
        estado = ESTADO_RECORDES;
        if (txtNomeLocal.text != "")
            SalvaRecordeLocal();
        paginaInsiraNome.SetActive(false);
        paginaRecordes.SetActive(true);

        MostraRecordes(scrollRecordes);
    }

    private void MostraRecordes(Transform pai)
    {
        // os recordes são armazenados em ordem, isso é feito no SalvaRecordeLocal(); por isso 'i' é a posição do recordista
        for (int i = 0; PlayerPrefs.HasKey("recordeLocal" + i); i++)
        {
            AdicionaRecordeView(pai, PlayerPrefs.GetString("nomeRecordeLocal" + i), PlayerPrefs.GetInt("recordeLocal" + i).ToString(), (i + 1).ToString(), (i + 1));
        }
    }
    public void Resetar()
    {
        PlayerPrefs.DeleteAll();
        Avisa("Recordes apagados");
    }

    public void AtivarTutorial()
    {
        PlayerPrefs.SetInt("tutorial", 0);
        Reiniciar();
    }

    public void SelecionaSkin(int qual)
    {
        PlayerPrefs.SetInt("skin", qual);
        Reiniciar();
    }

    public void SetVibracao()
    {
        if (!Hexagono_Controlador.vibracao)
        {
            txtBotaoVibracao.text = "Desligar vibração";
            PlayerPrefs.SetInt("vibracao", 1);
            Hexagono_Controlador.vibracao = true;
        }
        else
        {
            txtBotaoVibracao.text = "Ligar vibração";
            PlayerPrefs.SetInt("vibracao", 0);
            Hexagono_Controlador.vibracao = false;
        }
    }

    public void SetVibracao(bool ligado)
    {
        if (ligado)
        {
            txtBotaoVibracao.text = "Desligar vibração";
            Hexagono_Controlador.vibracao = true;
        }
        else
        {
            txtBotaoVibracao.text = "Ligar vibração";
            Hexagono_Controlador.vibracao = false;
        }
    }


    public void AdicionaRecordeView(
        Transform view,
        string nome,
        string recorde,
        string posicaoTexto,
        int posicao
    )
    {
        // se os recordes já estão instanciados, só atualiza os dados
        // obs: isso é feito uma vez por rodada, portanto não tem como existirem mais recordes salvos dos que já estão instanciados. 
        //      provavelmente, esses dados nunca precisam ser atualizados e esse loop do primeiro if é desnecessário.
        if (posicao < view.childCount)
        {
            // será utilizado 3 vezes, melhor criar a referencia
            UmRecorde component = view.GetChild(posicao).gameObject.GetComponent<UmRecorde>();
            // pega o nome do dicionario, remove acentos e atribui ao objeto
            component.nome.text = nome;
            // pega o recorde já como string
            component.recorde.text = recorde;
            // mostra a posição
            component.posicao.text = posicaoTexto;
        }
        else
        {
            // cria o objeto da tabela
            GameObject amigo = Instantiate(prefabRecorde);
            // coloca na lista pra ser exibido
            amigo.transform.SetParent(view);
            // conserta a escala
            amigo.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            // será utilizado 3 vezes, melhor criar a referencia
            UmRecorde component = amigo.GetComponent<UmRecorde>();
            component.transform.position = new Vector3(0, 0, 0);
            // pega o nome do dicionario, remove acentos e atribui ao objeto
            component.nome.text = nome;
            // pega o recorde já como string
            component.recorde.text = recorde;
            // mostra a posição
            component.posicao.text = posicaoTexto;
        }
    }

    public void BGFLash(Color cor)
    {
        spriteBackground.color = cor;
        animBackground.SetTrigger("Flash");
    }
}
