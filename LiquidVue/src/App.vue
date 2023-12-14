<template>
  <div class="page">
    <div class="header-container">
      <HeaderLiquid
          :orderBy="orderBy"
          :orderType="orderType"
          :orderByOptions="orderByOptions"
          :orderTypeOptions="orderTypeOptions"
          :genres="genres"
          @update:orderBy="updateOrderBy"
          @update:orderType="updateOrderType"
          @update:selectedGenre="updateSelectedGenre"
      />
    </div>
    <div class="content">
      <div class="grid-container">
        <GameInfo v-for="game in games" :key="game.rank" :game="game" class="game-info-item"/>
      </div>
    </div>
    <button @click="loadMoreGames" class="load-button">Load More Games</button>
  </div>
</template>

<script>
import axios from 'axios';
import GameInfo from "@/components/GameInfo.vue";
import HeaderLiquid from "@/components/HeaderLiquid.vue";

export default {
  components: {
    GameInfo,
    HeaderLiquid
  },
  data() {
    return {
      games: [],
      currentPage: 1,
      pageSize: 100,
      orderBy: 'Global_Sales',
      orderType:'Desc',
      selectedGenre: 'All',
      orderByOptions: ['Year', 'Global_Sales', 'EU_Sales', 'NA_Sales', 'JP_Sales', 'Other_Sales'],
      orderTypeOptions: ['Desc', 'Asc'],
      genres: []
    }
  },
  methods: {
    async fetchGames() {
      try {
        let endpoint = this.selectedGenre === 'All' ? '/Games' : `/Genres/${this.selectedGenre}`;
        const response = await axios.get(`http://localhost:5000${endpoint}?orderBy=${this.orderBy}&orderType=${this.orderType}&page=${this.currentPage}&pageSize=${this.pageSize}&filter=${this.filter}`);
        this.games = this.selectedGenre === 'All' ? response.data : response.data.games;
      } catch (error) {
        console.error('Error fetching games:', error);
      }
    },
    async loadMoreGames() {
      this.currentPage++;
      try {
        let endpoint = this.selectedGenre === 'All' ? '/Games' : `/Genres/${this.selectedGenre}`;
        const response = await axios.get(`http://localhost:5000${endpoint}?orderBy=${this.orderBy}&orderType=${this.orderType}&page=${this.currentPage}&pageSize=${this.pageSize}&filter=${this.filter}`);
        this.games = this.selectedGenre === 'All' ? [...this.games, ...response.data] : [...this.games, ...response.data.games];
      } catch (error) {
        console.error('Error loading more games:', error);
      }
    },
    updateOrderBy(value) {
      this.orderBy = value;
      this.fetchGames();
    },
    updateOrderType(value) {
      this.orderType = value;
      this.fetchGames();
    },
    updateSelectedGenre(value) {
      this.selectedGenre = value;
      this.fetchGames();
    },
    async fetchGenres() {
      try {
        const response = await axios.get('http://localhost:5000/Genres');
        this.genres = ['All', ...response.data];
      } catch (error) {
        console.error('Error fetching genres:', error);
      }
    }
  },
  mounted() {
    this.fetchGenres();
    this.fetchGames();
  }
}
</script>

<style scoped>
body {
  background-color: #EFEFEF;
}

.header-container {
  position: fixed;
  top: 0;
  width: 100%;
  z-index: 999;
}

.content {
  padding-top: 120px;
  margin-top: 30px;
}
.grid-container {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  grid-gap: 10px;
  grid-row-gap: 40px;
}
.game-info-item {
  padding: 10px;
  background-color: white;
}
.load-button {
  border: solid black 1px;
  display: block;
  margin: 0 auto;
  padding: 10px 20px;
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