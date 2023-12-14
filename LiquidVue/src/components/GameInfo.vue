<template>
  <div class="game-info">
    <div class="info">
      <div class="game-name">{{ game.gameName }}</div>
      <div class="platform">{{ game.platform }}</div>
      <div class="publisher">{{ game.publisher }}</div>
    </div>
    <div class="bottom-info">
      <span class="release-year">Year: <strong>{{ game.releaseYear }}</strong></span>
      <span class="rank">Rank: <strong>{{ game.rank }}</strong></span>
      <span class="global-sales">Sales: <strong>{{ displayedSales  }} Mil</strong></span>
    </div>
  </div>
</template>

<script>
export default {
  props: {
    game: {
      type: Object,
      required: true
    },
    orderBy: {
      type: String,
      required: true
    }
  },
  computed: {
    displayedSales() {
      switch (this.orderBy.toLowerCase()) {
        case 'global_sales':
          return this.game.global_Sales;
        case 'eu_sales':
          return this.game.eU_Sales;
        case 'na_sales':
          return this.game.nA_Sales;
        case 'jp_sales':
          return this.game.jP_Sales;
        case 'other_sales':
          return this.game.other_Sales;
        default:
          return this.game.global_Sales;
      }
    }
  },
}
</script>

<style scoped>
.game-info {
  border: solid black 1px;
  margin: 5px 40px;
  border-radius: 1px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  background-color: #D9D9D9;
}

.game-info .image-line {
  border: 0;
  border-top: 2px solid black;
  margin: 0 10px -40px;
}

.game-info .game-image {
  padding-top: 10px;
  width: 100%;
  max-height: 40%;
  height: auto;
  object-fit: contain;
}

.info {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  flex: 0 0 auto;
  margin-bottom: 20px;
}

.info .game-name {
  font-size: 2em;
  text-align: center;
  font-weight: bolder;
}

.info .platform {
  font-size: 1.5em;
  text-align: center;
}

.bottom-info {
  display: flex;
  justify-content: space-between;
  width: 100%;
}
</style>