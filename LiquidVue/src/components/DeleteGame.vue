<template>
  <div>
    <div v-if="showPopup" class="popup-container">
      <h1>Delete Game</h1>
      <div class="input-row">
        <label for="gameName">Game Name:</label>
        <input type="text" id="gameName" v-model="gameName" placeholder="Enter Game Name" required />
      </div>
      <div class="input-row">
        <label for="gameName">Platform:</label>
        <input type="text" id="gameName" v-model="platformName" placeholder="Enter Platform" required />
      </div>
      <div class="button-row">
        <button @click="saveGame" class="load-button">Delete</button>
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
      gameName: "",
      platformName:""
    };
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
      if (this.gameName === "" || this.platformName === "") {
        alert("Please supply a name and/or platform before deleting.");
        return;
      }
      try {
        await axios.delete(`http://localhost:5000/Games/${this.gameName}/${this.platformName}`);
        this.showPopup = false;
        this.$emit('closePopup');
        this.resetForm();
      } catch (error) {
        console.error("Error deleting game:", error);
      }
    },
    resetForm() {
      this.gameName = "";
      this.platformName="";
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
  flex: 0 0 100px;
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
</style>
