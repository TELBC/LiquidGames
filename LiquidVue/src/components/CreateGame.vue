<template>
  <div>
    <div v-if="showPopup" class="popup-container">
      <h1>Add Game</h1>
      <div class="input-row">
        <label for="gameName">Game Name:</label>
        <input type="text" id="gameName" v-model="game.GameName" placeholder="Enter Game Name" required />
      </div>
      <div class="input-row">
        <label for="platform">Platform:</label>
        <input type="text" id="platform" v-model="game.Platform" placeholder="Enter Platform" required />
      </div>
      <div class="input-row">
        <label for="publisher">Publisher:</label>
        <input type="text" id="publisher" v-model="game.Publisher" placeholder="Enter Publisher" required />
      </div>
      <div class="input-row">
        <label for="Genres">Genre:</label>
        <select v-model="selectedGenre" id="Genres" class="custom-dropdown">
          <option v-for="genre in genres" :value="genre">{{ genre }}</option>
        </select>
      </div>
      <div class="input-row">
        <label for="releaseYear">Release Year:</label>
        <input type="text" id="releaseYear" v-model="game.ReleaseYear" placeholder="Enter Release Year" required />
      </div>
      <div class="input-row">
        <label for="naSales">NA Sales:</label>
        <input type="text" id="naSales" v-model="game.NA_Sales" placeholder="Enter NA Sales" required />
      </div>
      <div class="input-row">
        <label for="euSales">EU Sales:</label>
        <input type="text" id="euSales" v-model="game.EU_Sales" placeholder="Enter EU Sales" required />
      </div>
      <div class="input-row">
        <label for="jpSales">JP Sales:</label>
        <input type="text" id="jpSales" v-model="game.JP_Sales" placeholder="Enter JP Sales" required />
      </div>
      <div class="input-row">
        <label for="otherSales">Other Sales:</label>
        <input type="text" id="otherSales" v-model="game.Other_Sales" placeholder="Enter Other Sales" required />
      </div>
      <div class="button-row">
        <button @click="saveGame" class="load-button">Save</button>
        <button @click="closePopup" class="load-button">Cancel</button>
      </div>
    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  data() {
    return {
      showPopup: false,
      genres:[],
      selectedGenre:'',
      game: {
        GameName: "",
        Platform: "",
        ReleaseYear: new Date().getFullYear(),
        Publisher: "",
        NA_Sales: 0,
        EU_Sales: 0,
        JP_Sales: 0,
        Other_Sales: 0,
      },
    };
  },
  mounted() {
    this.fetchGenres();
  },
  methods: {
    async fetchGenres() {
      try {
        const response = await axios.get('http://localhost:5000/Genres');
        this.genres = response.data;
      } catch (error) {
        console.error('Error fetching genres:', error);
      }
    },
    show() {
      this.showPopup = true;
    },
    closePopup() {
      this.showPopup = false;
      this.$emit('close');
    },
    async saveGame() {
      const isEmptyField = Object.values(this.game).some(value => value === "" || value === 0);
      if (isEmptyField) {
        alert("Please fill in all fields before saving.");
        return;
      }

      this.game.NA_Sales = parseFloat(this.game.NA_Sales);
      this.game.EU_Sales = parseFloat(this.game.EU_Sales);
      this.game.JP_Sales = parseFloat(this.game.JP_Sales);
      this.game.Other_Sales = parseFloat(this.game.Other_Sales);

      // Check if parsed values are valid numbers
      if (
          isNaN(this.game.NA_Sales) ||
          isNaN(this.game.EU_Sales) ||
          isNaN(this.game.JP_Sales) ||
          isNaN(this.game.Other_Sales)
      ) {
        alert("Please enter valid numeric values for sales.");
        return;
      }

      const summed_Global_Sales =
          this.game.NA_Sales +
          this.game.EU_Sales +
          this.game.JP_Sales +
          this.game.Other_Sales;
      this.game.Global_Sales = parseFloat(summed_Global_Sales.toFixed(2));
      try {
        await axios.post(`http://localhost:5000/Genres/addGame/${this.selectedGenre}`, this.game);
        this.showPopup = false;
        this.$emit('closePopup');
        this.resetForm();
      } catch (error) {
        console.error("Error saving game:", error);
      }
    },
    resetForm() {
      this.game = {
        Rank: 0,
        GameName: "",
        Platform: "",
        ReleaseYear: new Date().getFullYear(),
        Publisher: "",
        NA_Sales: 0,
        EU_Sales: 0,
        JP_Sales: 0,
        Other_Sales: 0,
        Global_Sales: 0,
      };
    },
  },
};
</script>

<style scoped>
.popup-container {
  position: fixed;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  background-color: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.2);
  outline: 1px solid black;
  z-index: 10000;
}

.input-row {
  display: flex;
  align-items: center;
  margin-bottom: 10px;
}

.input-row label {
  flex: 0 0 120px;
  margin-right: 10px;
}

.button-row {
  display: flex;
  justify-content: center;
  margin-top: 20px;
}

.button-row button {
  margin: 0 5px;
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
.custom-dropdown {
  display: flex;
  align-items: center;
  background-color: white;
  width: fit-content;
  min-width: 163px;
}

.custom-dropdown select {
  width: 100%;
  box-sizing: border-box;
  background-color: white;
  border: none;
  border-radius: 0;
  padding: 8px;
}
</style>