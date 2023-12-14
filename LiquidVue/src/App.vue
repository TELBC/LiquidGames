<template>
  <div class="page">
    <div class="header-container">
      <HeaderLiquid/>
    </div>
    <div class="content">
      <div class="grid-container">
        <GameInfo v-for="game in games" :key="game.Rank" :game="game" class="game-info-item"/>
      </div>
    </div>
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
      games: []
    }
  },
  mounted() {
    axios.get('http://localhost:5000/Games')
        .then(response => {
          this.games = response.data;
        });
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
</style>