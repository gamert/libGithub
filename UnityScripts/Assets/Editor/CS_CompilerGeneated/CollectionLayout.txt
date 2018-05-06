
/*
// Is alternative restored code with simple decompilation options:
using Abrakam;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public class CollectionLayout : MonoBehaviour
{
    public enum CollectionMode
    {
        COLLECTION,
        DECK_EDITION,
        NONE
    }

    [CompilerGenerated]
    private sealed class _003CTransitionToDeckEditionCoroutine_003Ec__Iterator0 : IEnumerator, IDisposable, IEnumerator<object>
    {
        internal CollectionSceneLayout _003ClocCollectionSceneLayout_003E__0;

        internal Account _003ClocAccount_003E__0;

        internal Deck parDeck;

        internal CollectionLayout _0024this;

        internal object _0024current;

        internal bool _0024disposing;

        internal int _0024PC;

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this._0024current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this._0024current;
            }
        }

        [DebuggerHidden]
        public _003CTransitionToDeckEditionCoroutine_003Ec__Iterator0()
        {
        }

        public bool MoveNext()
        {
            uint num = (uint)this._0024PC;
            this._0024PC = -1;
            switch (num)
            {
                case 0u:
                    this._003ClocCollectionSceneLayout_003E__0 = (ApplicationManager.faeriaSceneManager.GetSceneLayout(FaeriaSceneManager.SceneType.COLLECTION) as CollectionSceneLayout);
                    this._003ClocAccount_003E__0 = ApplicationManager.dataManager.GetYouAccount();
                    this._003ClocAccount_003E__0.currentGameMode = GameMode.COLLECTION;
                    this._0024this.editedDeck = this.parDeck;
                    this._0024this.UpdateMode(CollectionMode.DECK_EDITION);
                    this._0024current = null;
                    if (!this._0024disposing)
                    {
                        this._0024PC = 1;
                    }
                    return true;
                case 1u:
                    this._0024PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Dispose()
        {
            this._0024disposing = true;
            this._0024PC = -1;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }
    }

    [CompilerGenerated]
    private sealed class _003CForceDeckTutorialCoroutine_003Ec__Iterator1 : IEnumerator, IDisposable, IEnumerator<object>
    {
        internal CollectionLayout _0024this;

        internal object _0024current;

        internal bool _0024disposing;

        internal int _0024PC;

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this._0024current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this._0024current;
            }
        }

        [DebuggerHidden]
        public _003CForceDeckTutorialCoroutine_003Ec__Iterator1()
        {
        }

        public bool MoveNext()
        {
            uint num = (uint)this._0024PC;
            this._0024PC = -1;
            switch (num)
            {
                case 0u:
                    ApplicationManager.gameStateManager.HandleMessage(MessageType.GO_TO_COLLECTION);
                    this._0024current = SceneLayout.WaitUntilInitialised(FaeriaSceneManager.SceneType.COLLECTION);
                    if (!this._0024disposing)
                    {
                        this._0024PC = 1;
                    }
                    return true;
                case 1u:
                    this._0024this.OnCreateDeck();
                    this._0024PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Dispose()
        {
            this._0024disposing = true;
            this._0024PC = -1;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }
    }

    [Header("Buttons")]
    [SerializeField]
    private FaeriaButtonDock craftModeButton;

    [SerializeField]
    private FaeriaButtonDock exitCraftModeButton;

    [SerializeField]
    private FaeriaButtonDock createDeckButton;

    [SerializeField]
    private FaeriaButtonDock saveDeckButton;

    [Header("Panels")]
    [SerializeField]
    private RectTransform deckEditionToolsPanel;

    [SerializeField]
    private CardCollectionLayout cardCollectionPanel;

    [SerializeField]
    private CardZoomLayout cardZoomLayout;

    [SerializeField]
    private DeckMiniListLayout deckMiniListLayout;

    [Header("Tooltips")]
    [SerializeField]
    private Tooltipable craftTooltip;

    [Header("Docks")]
    [SerializeField]
    private RectTransform deckListLayoutDock;

    [SerializeField]
    private RectTransform craftLayoutDock;

    [SerializeField]
    private RectTransform blueprintSelectionDock;

    [Header("Prefabs")]
    [SerializeField]
    private DeckListLayout deckListLayoutPrefab;

    [SerializeField]
    private CraftCommonLayout craftLayoutPrefab;

    [SerializeField]
    private BlueprintSelectionLayout blueprintSelectionPrefab;

    private CraftCommonLayout craftLayout;

    private DeckContentLayout deckContentLayout;

    private BlueprintSelectionLayout blueprintSelectionLayout;

    [HideInInspector]
    public bool hasDeckBeenModified;

    [HideInInspector]
    public CollectionMode currentCollectionMode;

    private bool isCrafting;

    private DataRoomDecks dataRoomDecks;

    private DataRoomTournamentDecks dataRoomTournamentDecks;

    private Deck editedDeck;

    private Deck deckToDelete;

    private void Awake()
    {
        DeckListLayout deckListLayout = UnityEngine.Object.Instantiate(this.deckListLayoutPrefab);
        deckListLayout.transform.SetParent(this.deckListLayoutDock, false);
        this.deckContentLayout = deckListLayout.deckContentLayout;
        this.deckContentLayout.cardZoomLayout = this.cardZoomLayout;
        this.deckContentLayout.deckInfoPanel.gameObject.SetActive(true);
        this.craftLayout = UnityEngine.Object.Instantiate(this.craftLayoutPrefab);
        this.craftLayout.transform.SetParent(this.craftLayoutDock, false);
        this.craftLayout.craftCardPanel.donePanel.gameObject.SetActive(true);
        this.craftLayout.craftCardPanel.AddDoneListener(new UnityAction(this.ToggleCrafting));
        this.blueprintSelectionLayout = UnityEngine.Object.Instantiate(this.blueprintSelectionPrefab);
        this.blueprintSelectionLayout.transform.SetParent(this.blueprintSelectionDock, false);
        this.blueprintSelectionLayout.gameObject.SetActive(false);
        this.blueprintSelectionLayout.SetCollectionLayout(this);
        this.craftModeButton.AddClickListener(new UnityAction(this.ToggleCrafting));
        this.exitCraftModeButton.AddClickListener(new UnityAction(this.ToggleCrafting));
        this.createDeckButton.AddClickListener(new UnityAction(this.OnCreateDeck));
        this.saveDeckButton.AddClickListener(new UnityAction(this.OnSaveDeck));
        if (this.dataRoomDecks == null)
        {
            this.dataRoomDecks = (DataRoomDecks)ApplicationManager.dataManager.GetRoom("decks");
        }
        if (this.dataRoomTournamentDecks == null)
        {
            this.dataRoomTournamentDecks = (DataRoomTournamentDecks)ApplicationManager.dataManager.GetRoom("tournamentDecks");
        }
    }

    private void OnEnable()
    {
        GuiManager guiManager = ApplicationManager.applicationManager.guiManager;
        TopBarLayout topBarGui = guiManager.GetTopBarGui();
        Account youAccount = ApplicationManager.dataManager.GetYouAccount();
        topBarGui.onBackButtonActions += new Action(this.OnBackButton);
        if (!youAccount.HasAccessToCrafting())
        {
            DataRoomProperties dataRoomProperties = (DataRoomProperties)ApplicationManager.dataManager.GetRoom("properties");
            int num = int.Parse(dataRoomProperties.GetProperty("levelRequirementForCrafting").value);
            this.craftModeButton.interactable = false;
            this.exitCraftModeButton.interactable = false;
            this.craftTooltip.gameObject.SetActive(true);
            this.craftTooltip.needLocalisation = false;
            this.craftTooltip.message = LocalisationManager.GetLocalisedValue("CRAFT_MODE_UNAVAILABLE_TOOTIP", num);
        }
        else
        {
            this.craftModeButton.interactable = true;
            this.exitCraftModeButton.interactable = true;
            this.craftTooltip.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        GuiManager guiManager = ApplicationManager.applicationManager.guiManager;
        TopBarLayout topBarGui = guiManager.GetTopBarGui();
        topBarGui.onBackButtonActions -= new Action(this.OnBackButton);
        if (this.currentCollectionMode == CollectionMode.DECK_EDITION)
        {
            if (this.hasDeckBeenModified)
            {
                DialogManager dialogManager = ApplicationManager.applicationManager.guiManager.GetDialogManager();
                dialogManager.ShowOkCancelPrompt("SAVE_DECK_FIRST", delegate
                {
                    this.deckContentLayout.SaveDeck(null);
                }, delegate
                {
                    this.deckContentLayout.OnCancelModification(null);
                });
            }
            else
            {
                this.deckContentLayout.OnCancelModification(null);
            }
        }
    }

    public void Initialise(Deck parDeckToOpenForEdition = null)
    {
        this.cardCollectionPanel.collectionLayout = this;
        this.cardCollectionPanel.maxDeckSize = this.deckContentLayout.deckSize;
        this.deckContentLayout.collectionLayout = this;
        this.craftLayout.SetCollectionLayout(this);
        this.craftLayout.SetCurrentMode(CraftCommonLayout.CraftMode.CARD);
        this.hasDeckBeenModified = false;
        foreach (Deck data in this.dataRoomDecks.GetDatas())
        {
            if (!data.isHidden)
            {
                this.AddDeckLayout(data);
            }
        }
        if (parDeckToOpenForEdition != null)
        {
            this.EditDeck(parDeckToOpenForEdition);
            this.hasDeckBeenModified = true;
        }
        else
        {
            this.ShowCollection();
        }
    }

    private void UpdateMode(CollectionMode parCurrentCollectionMode)
    {
        this.blueprintSelectionLayout.gameObject.SetActive(false);
        this.deckEditionToolsPanel.gameObject.SetActive(false);
        this.deckMiniListLayout.gameObject.SetActive(false);
        this.craftLayoutDock.gameObject.SetActive(false);
        this.exitCraftModeButton.gameObject.SetActive(false);
        this.currentCollectionMode = parCurrentCollectionMode;
        this.craftModeButton.gameObject.SetActive(true);
        this.isCrafting = false;
        switch (parCurrentCollectionMode)
        {
            case CollectionMode.COLLECTION:
                this.deckMiniListLayout.gameObject.SetActive(true);
                this.UpdateCreateDeckButtonInteractability();
                this.cardCollectionPanel.Initialise(CardCollectionLayout.Mode.COLLECTION);
                this.editedDeck = null;
                break;
            case CollectionMode.DECK_EDITION:
                this.deckEditionToolsPanel.gameObject.SetActive(true);
                this.cardCollectionPanel.Initialise(CardCollectionLayout.Mode.DECK_EDITION);
                this.saveDeckButton.interactable = (this.editedDeck.id >= 0);
                this.deckContentLayout.EditDeck(this.editedDeck);
                ApplicationManager.gameStateManager.SetDeckSelectedForEdition(null);
                break;
        }
    }

    public void Finalise()
    {
        this.cardCollectionPanel.Finalise();
    }

    public void UpdateCardAvailability()
    {
        this.cardCollectionPanel.UpdateCardAvailability();
    }

    public void AddDeckLayout(Deck parDeck)
    {
        if (!parDeck.isHidden)
        {
            this.deckMiniListLayout.AddDeck(parDeck, new Action<Deck>(this.OnDeckButtonClicked), new Action<Deck>(this.OnEditDeck), new Action<Deck>(this.OnDeleteDeck));
            this.UpdateCreateDeckButtonInteractability();
        }
    }

    public void ConfirmDeck(long parDeckId)
    {
        this.editedDeck.UpdateId(parDeckId);
        this.saveDeckButton.interactable = true;
    }

    public void OnSaveDeck()
    {
        this.deckContentLayout.OnSaveDeck();
    }

    private void OnBackButton()
    {
        if (this.currentCollectionMode == CollectionMode.DECK_EDITION)
        {
            if (this.hasDeckBeenModified)
            {
                GuiManager guiManager = ApplicationManager.applicationManager.guiManager;
                DialogManager dialogManager = guiManager.GetDialogManager();
                dialogManager.ShowOkCancelPrompt("MODIFICATION_CANCEL_CONFIRMATION", delegate
                {
                    this.deckContentLayout.OnCancelModification(new Action(this.ShowCollection));
                }, new Action(this.OnCancelModificationCancel));
            }
            else
            {
                this.deckContentLayout.OnCancelModification(new Action(this.ShowCollection));
            }
        }
        else
        {
            ApplicationManager.gameStateManager.HandleMessage(MessageType.GO_TO_HOME);
        }
    }

    private void OnCancelModificationCancel()
    {
    }

    public void PickCraftCard(Card parCard)
    {
        DataRoomCollection dataRoomCollection = (DataRoomCollection)ApplicationManager.dataManager.GetRoom("collection");
        Item item = dataRoomCollection.GetItem(parCard);
        this.craftLayout.UpdateCard(item);
    }

    public void DeckIn(Card parCard)
    {
        bool flag = parCard.IsGold();
        this.deckContentLayout.AddCard(parCard);
        ApplicationManager.worldNetworkManager.AppendParameter("cardId", parCard.id.ToString());
        WorldNetworkManager worldNetworkManager = ApplicationManager.worldNetworkManager;
        int num = flag ? 1 : 0;
        worldNetworkManager.AppendParameter("isGold", num.ToString());
        ApplicationManager.worldNetworkManager.SendCommand("deckIn");
        this.hasDeckBeenModified = true;
    }

    public void DeckOut(Card parCard)
    {
        bool flag = parCard.IsGold();
        this.deckContentLayout.RemoveCard(parCard);
        ApplicationManager.worldNetworkManager.AppendParameter("cardId", parCard.id.ToString());
        WorldNetworkManager worldNetworkManager = ApplicationManager.worldNetworkManager;
        int num = flag ? 1 : 0;
        worldNetworkManager.AppendParameter("isGold", num.ToString());
        ApplicationManager.worldNetworkManager.SendCommand("deckOut");
        this.hasDeckBeenModified = true;
    }

    public DeckContentLayout GetDeckContentLayout()
    {
        return this.deckContentLayout;
    }

    private void OnDeckButtonClicked(Deck parDeck)
    {
    }

    private void OnEditDeck(Deck parDeck)
    {
        if (this.dataRoomTournamentDecks != null && this.dataRoomTournamentDecks.GetDatas().Contains(parDeck))
        {
            string localisedValue = LocalisationManager.GetLocalisedValue("TOURNAMENT_DECK_LOCKED");
            ApplicationManager.applicationManager.guiManager.notificationsLayout.DisplayMessage(localisedValue, false);
        }
        else
        {
            ApplicationManager.gameStateManager.CheckCancelGameSearches(delegate
            {
                this.EditDeck(parDeck);
            });
        }
    }

    public void OnCreateDeck()
    {
        ApplicationManager.gameStateManager.CheckCancelGameSearches(delegate
        {
            this.blueprintSelectionLayout.gameObject.SetActive(true);
        });
    }

    private bool IsMaxDecksLimitReached()
    {
        return this.dataRoomDecks.GetDeckCount() >= 12;
    }

    private void UpdateCreateDeckButtonInteractability()
    {
        this.createDeckButton.interactable = !this.IsMaxDecksLimitReached();
    }

    public void CreateDeck()
    {
        if (!this.IsMaxDecksLimitReached())
        {
            string localisedValue = LocalisationManager.GetLocalisedValue("NEW_DECK_NAME");
            Deck deck = new Deck(-1L);
            deck.isHidden = false;
            deck.name = localisedValue;
            ApplicationManager.worldNetworkManager.AppendParameter("name", localisedValue);
            ApplicationManager.worldNetworkManager.SendCommand("createDeck");
            this.OnEditDeck(deck);
        }
    }

    public void OnDeleteDeck(Deck parDeck)
    {
        if (this.dataRoomTournamentDecks.GetDatas().Contains(parDeck))
        {
            string localisedValue = LocalisationManager.GetLocalisedValue("TOURNAMENT_DECK_LOCKED");
            ApplicationManager.applicationManager.guiManager.notificationsLayout.DisplayMessage(localisedValue, false);
        }
        else
        {
            GuiManager guiManager = ApplicationManager.applicationManager.guiManager;
            DialogManager dialogManager = guiManager.GetDialogManager();
            this.deckToDelete = parDeck;
            dialogManager.ShowOkCancelPrompt("DELETE_DECK_CONFIRMATION", new Action(this.OnTryDeleteDeck), new Action(this.OnDeleteDeckCancel));
        }
    }

    private void OnTryDeleteDeck()
    {
        Action parExecuteAction = new Action(this.OnDeleteDeck);
        if (this.deckContentLayout.IsEditingNewDeck(this.deckToDelete))
        {
            parExecuteAction = new Action(this.OnCancelDeck);
        }
        ApplicationManager.gameStateManager.CheckCancelGameSearches(parExecuteAction);
    }

    private void OnDeleteDeck()
    {
        DataManager dataManager = ApplicationManager.dataManager;
        ApplicationManager.worldNetworkManager.AppendParameter("deckId", this.deckToDelete.id.ToString());
        ApplicationManager.worldNetworkManager.SendCommand("deleteDeck");
        this.dataRoomDecks.RemoveData(this.deckToDelete.id);
        dataManager.RemoveData(this.deckToDelete.id, typeof(Deck));
        this.deckMiniListLayout.RemoveDeck(this.deckToDelete.id);
        if (this.currentCollectionMode == CollectionMode.DECK_EDITION)
        {
            this.deckContentLayout.ExitDeck();
        }
        this.UpdateCreateDeckButtonInteractability();
        this.deckToDelete = null;
        if (this.currentCollectionMode != 0)
        {
            this.ShowCollection();
        }
    }

    private void OnCancelDeck()
    {
        this.deckContentLayout.OnCancelModification(null);
        if (this.currentCollectionMode != 0)
        {
            this.ShowCollection();
        }
    }

    private void OnDeleteDeckCancel()
    {
        this.deckToDelete = null;
    }

    public void EditDeck(Deck parDeck)
    {
        CollectionSceneLayout collectionSceneLayout = ApplicationManager.faeriaSceneManager.GetSceneLayout(FaeriaSceneManager.SceneType.COLLECTION) as CollectionSceneLayout;
        collectionSceneLayout.StartCoroutine(this.TransitionToDeckEditionCoroutine(parDeck));
    }

    public void ShowCollection()
    {
        this.UpdateMode(CollectionMode.COLLECTION);
    }

    public void UpdateCardCollectionPanel(CraftCommonLayout.CraftMode parCraftMode)
    {
        this.cardCollectionPanel.UpdateCards(parCraftMode);
    }

    [DebuggerHidden]
    private IEnumerator TransitionToDeckEditionCoroutine(Deck parDeck)
    {
        _003CTransitionToDeckEditionCoroutine_003Ec__Iterator0 _003CTransitionToDeckEditionCoroutine_003Ec__Iterator = new _003CTransitionToDeckEditionCoroutine_003Ec__Iterator0();
        _003CTransitionToDeckEditionCoroutine_003Ec__Iterator.parDeck = parDeck;
        _003CTransitionToDeckEditionCoroutine_003Ec__Iterator._0024this = this;
        return _003CTransitionToDeckEditionCoroutine_003Ec__Iterator;
    }

    [DebuggerHidden]
    public IEnumerator ForceDeckTutorialCoroutine()
    {
        _003CForceDeckTutorialCoroutine_003Ec__Iterator1 _003CForceDeckTutorialCoroutine_003Ec__Iterator = new _003CForceDeckTutorialCoroutine_003Ec__Iterator1();
        _003CForceDeckTutorialCoroutine_003Ec__Iterator._0024this = this;
        return _003CForceDeckTutorialCoroutine_003Ec__Iterator;
    }

    public void ToggleCrafting()
    {
        this.isCrafting = !this.isCrafting;
        this.cardCollectionPanel.EnableGoldCardFilter(!this.isCrafting);
        if (this.isCrafting)
        {
            this.craftLayoutDock.gameObject.SetActive(true);
            this.craftModeButton.gameObject.SetActive(false);
            this.exitCraftModeButton.gameObject.SetActive(true);
            this.cardCollectionPanel.Initialise(CardCollectionLayout.Mode.CRAFT);
        }
        else
        {
            this.craftLayoutDock.gameObject.SetActive(false);
            this.exitCraftModeButton.gameObject.SetActive(false);
            this.craftModeButton.gameObject.SetActive(true);
            CardCollectionLayout.Mode parMode = CardCollectionLayout.Mode.COLLECTION;
            if (this.currentCollectionMode == CollectionMode.DECK_EDITION)
            {
                parMode = CardCollectionLayout.Mode.DECK_EDITION;
            }
            this.cardCollectionPanel.Initialise(parMode);
        }
    }

    public bool IsCrafting()
    {
        return this.isCrafting;
    }

    public bool IsBlueprintSelectionScreenDisplayed()
    {
        return this.blueprintSelectionLayout.gameObject.activeInHierarchy;
    }

    public bool IsCollectionScreenDisplayed()
    {
        return !this.blueprintSelectionLayout.gameObject.activeInHierarchy && !this.deckContentLayout.gameObject.activeInHierarchy;
    }

    public bool IsDeckSelectionScreenDisplayed()
    {
        return this.deckMiniListLayout.gameObject.activeInHierarchy;
    }

    public bool IsDeckEditionScreenDisplayed()
    {
        return this.deckContentLayout.gameObject.activeInHierarchy;
    }

    public bool IsInCraftingMode()
    {
        return this.craftLayout.playerDustCountText.gameObject.activeInHierarchy;
    }

    public void ForceScrollTo(Card parCard)
    {
        this.cardCollectionPanel.ForceScrollTo(parCard);
    }

    public void ResetScrollbar()
    {
        this.cardCollectionPanel.ResetScrollbar();
    }
}

*/

