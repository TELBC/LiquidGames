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
    <div class="line"/>
  </div>
</template>

<script>
export default {
  props: {
    orderBy: String,
    orderType: String,
    orderByOptions: Array,
    orderTypeOptions: Array,
    genres: Array
  },
  data() {
    return {
      selectedGenre: 'All'
    };
  },
  methods: {
    updateOrderBy(event) {
      this.$emit('update:orderBy', event.target.value);
    },
    updateOrderType(event) {
      this.$emit('update:orderType', event.target.value);
    },
    updateSelectedGenre(event) {
      this.$emit('update:selectedGenre', event.target.value);
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
}

.load-button {
  border: solid black 1px;
  display: block;
  margin: 0 0 5px;
  padding: 5px 10px;
  font-size: 12px;
  width: 100px;
  background-color: #e8e8e8;
  color: black;
  border-radius: 5px;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.load-button:hover {
  background-color: #cbcbcb;
}
</style>