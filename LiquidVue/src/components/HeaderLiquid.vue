<template>
  <div class="header">
    <div class="logo-container">
      <img class="water-drop" src="@/assets/water-drop.png" alt="waterdrop">
      <div class="title-container">
        <div class="text">
          <div class="liquid">LIQUID</div>
          <div class="games">Games</div>
        </div>
      </div>
      <div class="button-container">
        <div class="button-wrapper">
          <button @click="openCreatePopup" class="load-button">Add</button>
          <CreateGame ref="gamecreatePopup" @close="closePopup" @closePopup="isPopupOpen = false" />
        </div>
        <div class="button-wrapper">
          <button @click="openDeletePopup" class="load-button">Delete</button>
          <DeleteGame ref="gamedeletePopup" @close="closePopup" @closePopup="isPopupOpen = false"/>
        </div>
      </div>
      <div class="search-container">
        <input type="text" v-model="searchTerm" @input="handleInput" placeholder="Search for similar games...">
      </div>
      <div class="dropdowns">
        <select v-model="selectedGenre" @change="updateSelectedGenre" class="load-button">
          <option v-for="genre in genres" :value="genre">{{ genre }}</option>
        </select>
        <select @change="updateOrderBy" class="load-button">
          <option v-for="option in orderByOptions" :value="option">{{ option }}</option>
        </select>
        <select @change="updateOrderType" class="load-button">
          <option v-for="option in orderTypeOptions" :value="option">{{ option }}</option>
        </select>
      </div>
    </div>
    <div class="overlay" v-if="isPopupOpen"></div>
    <div class="line"/>
  </div>
</template>

<script>
import CreateGame from "@/components/CreateGame.vue";
import DeleteGame from "@/components/DeleteGame.vue";
export default {
  components: {
    CreateGame,
    DeleteGame
  },
  props: {
    orderBy: String,
    orderType: String,
    orderByOptions: Array,
    orderTypeOptions: Array,
    genres: Array
  },
  data() {
    return {
      selectedGenre: 'All',
      searchTerm: '',
      delayTimer: null,
      isPopupOpen: false
    };
  },
  methods: {
    openCreatePopup() {
      this.isPopupOpen = true;
      this.$refs.gamecreatePopup.show();
    },
    openDeletePopup() {
      this.isPopupOpen = true;
      this.$refs.gamedeletePopup.show();
    },
    closePopup() {
      this.isPopupOpen = false;
    },
    handleInput() {
      clearTimeout(this.delayTimer);
      this.delayTimer = setTimeout(() => {
        this.searchGames();
      }, 200);
    },
    updateOrderBy(event) {
      this.$emit('update:orderBy', event.target.value);
    },
    updateOrderType(event) {
      this.$emit('update:orderType', event.target.value);
    },
    updateSelectedGenre(event) {
      this.$emit('update:selectedGenre', event.target.value);
    },
    searchGames() {
      this.$emit('searchGames', this.searchTerm);
    }
  }
};
</script>

<style scoped>
.header {
  width: 100vw;
  background-color: #e8e8e8;
  margin-bottom: 20px;
}

.logo-container {
  display: flex;
  align-items: center;
}

.title-container {
  width: 100vw;
  color: black;
}

.text {
  display: flex;
}

.liquid {
  font-size: 9vh;
  margin-right: 5px;
}

.games {
  padding-top: 4.5vh;
  font-size: 4vh;
}

.water-drop {
  height: 90px;
  width: auto;
  padding: 20px;
}

.line {
  width: 100vw;
  background-color: #9c9b9b;
  height: 2px;
}

.dropdowns {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  margin-right: 20px;
  gap: 5px;
}

.load-button {
  border: solid black 1px;
  display: block;
  margin: 0 0 5px;
  padding: 5px 10px;
  font-size: 12px;
  min-width: 100px;
  width: auto;
  background-color: white;
  color: black;
  border-radius: 5px;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.load-button:hover {
  background-color: white;
}
.search-container {
  margin: 10px;
}
.search-container input[type="text"] {
  border: solid black 1px;
  padding: 8px;
  border-radius: 4px;
  width: 250px;
}
.button-container {
  display: flex;
  flex-direction: column;
}

.button-wrapper {
  display: flex;
  align-items: center;
  width: 100%;
  margin-bottom: 5px;
}

.load-button {
  flex: 1;
  width: 100%; /* Adjust width */
  margin: 0;
}
.overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  z-index: 9999;
}
</style>